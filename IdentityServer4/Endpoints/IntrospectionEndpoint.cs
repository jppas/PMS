﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using IdentityServer4.Validation;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using IdentityServer4.Hosting;
using IdentityServer4.Endpoints.Results;
using Microsoft.AspNetCore.Http;

namespace IdentityServer4.Endpoints
{
    public class IntrospectionEndpoint : IEndpoint
    {
        private readonly IEventService _events;
        private readonly IIntrospectionResponseGenerator _generator;
        private readonly ILogger<IntrospectionEndpoint> _logger;
        private readonly IIntrospectionRequestValidator _requestValidator;
        private readonly ApiSecretValidator _apiSecretValidator;

        public IntrospectionEndpoint(ApiSecretValidator apiSecretValidator, IIntrospectionRequestValidator requestValidator, IIntrospectionResponseGenerator generator, IEventService events, ILogger<IntrospectionEndpoint> logger)
        {
            _apiSecretValidator = apiSecretValidator;
            _requestValidator = requestValidator;
            _generator = generator;
            _events = events;
            _logger = logger;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            _logger.LogTrace("Processing introspection request.");

            // validate HTTP
            if (context.Request.Method != "POST")
            {
                _logger.LogWarning("Introspection endpoint only supports POST requests");
                return new StatusCodeResult(405);
            }

            return await ProcessIntrospectionRequestAsync(context);
        }

        private async Task<IEndpointResult> ProcessIntrospectionRequestAsync(HttpContext context)
        {
            _logger.LogDebug("Starting introspection request.");

            var apiResult = await _apiSecretValidator.ValidateAsync(context);
            if (apiResult.Resource == null)
            {
                _logger.LogError("API unauthorized to call introspection endpoint. aborting.");
                return new StatusCodeResult(401);
            }

            var parameters = (await context.Request.ReadFormAsync()).AsNameValueCollection();

            var validationResult = await _requestValidator.ValidateAsync(parameters, apiResult.Resource);
            var response = await _generator.ProcessAsync(validationResult, apiResult.Resource);

            if (validationResult.IsActive)
            {
                LogSuccess(validationResult.Token, "active", apiResult.Resource.Name);
                return new IntrospectionResult(response);
            }

            if (validationResult.IsError)
            {
                if (validationResult.FailureReason == IntrospectionRequestValidationFailureReason.MissingToken)
                {
                    LogFailure(validationResult.ErrorDescription, validationResult.Token, apiResult.Resource.Name);
                    return new BadRequestResult("missing_token");
                }

                if (validationResult.FailureReason == IntrospectionRequestValidationFailureReason.InvalidToken)
                {
                    LogSuccess(validationResult.Token, "inactive", apiResult.Resource.Name);
                    return new IntrospectionResult(response);
                }

                if (validationResult.FailureReason == IntrospectionRequestValidationFailureReason.InvalidScope)
                {
                    LogFailure("API not authorized to introspect token", validationResult.Token, apiResult.Resource.Name);
                    return new IntrospectionResult(response);
                }
            }

            _logger.LogError("Invalid token introspection outcome");
            throw new InvalidOperationException("Invalid token introspection outcome");
        }

        private void LogSuccess(string token, string tokenStatus, string apiName)
        {
            _logger.LogInformation("Success token introspection. Token status: {tokenStatus}, for API name: {apiName}", tokenStatus, apiName);
        }

        private void LogFailure(string error, string token, string apiName)
        {
            _logger.LogError("Failed token introspection: {error}, for API name: {apiName}", error, apiName);
        }
    }
}