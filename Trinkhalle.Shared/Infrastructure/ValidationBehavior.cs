using FluentResults;
using FluentValidation;
using MediatR;

namespace Trinkhalle.Shared.Infrastructure
{
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : class, IRequest<TResponse>
        where TResponse : ResultBase, new()
    {
        private readonly IValidator<TRequest> _validator;

        public ValidationBehavior(IValidator<TRequest> validator)
        {
            _validator = validator;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (validationResult.IsValid) return await next();

            var result = new TResponse();
            result.Reasons.Add(new Error("Validation failed"));

            return result;
        }
    }
}