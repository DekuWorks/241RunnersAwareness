using FluentValidation;
using _241RunnersAPI.Models;

namespace _241RunnersAPI.Validators
{
    /// <summary>
    /// FluentValidation validator for user registration
    /// </summary>
    public class UserRegistrationValidator : AbstractValidator<UserRegistrationDto>
    {
        public UserRegistrationValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .Must(BeUniqueEmail).WithMessage("Email is already registered");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("First name can only contain letters, spaces, hyphens, apostrophes, and periods");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Last name can only contain letters, spaces, hyphens, apostrophes, and periods");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .Must(BeValidRole).WithMessage("Role must be one of: user, parent, caregiver, therapist, adoptiveparent, admin");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^[\+]?[1-9][\d]{0,15}$").WithMessage("Please enter a valid phone number")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.City)
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("City can only contain letters, spaces, hyphens, apostrophes, and periods")
                .When(x => !string.IsNullOrEmpty(x.City));

            RuleFor(x => x.State)
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("State can only contain letters, spaces, hyphens, apostrophes, and periods")
                .When(x => !string.IsNullOrEmpty(x.State));

            RuleFor(x => x.ZipCode)
                .Matches(@"^[\d\-]+$").WithMessage("Zip code can only contain numbers and hyphens")
                .When(x => !string.IsNullOrEmpty(x.ZipCode));

            RuleFor(x => x.Organization)
                .Matches(@"^[a-zA-Z0-9\s\-'\.&()]+$").WithMessage("Organization can only contain letters, numbers, spaces, hyphens, apostrophes, periods, ampersands, and parentheses")
                .When(x => !string.IsNullOrEmpty(x.Organization));

            RuleFor(x => x.Title)
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Title can only contain letters, spaces, hyphens, apostrophes, and periods")
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Credentials)
                .Matches(@"^[a-zA-Z0-9\s\-'.,&()]+$").WithMessage("Credentials can only contain letters, numbers, spaces, hyphens, apostrophes, commas, periods, ampersands, and parentheses")
                .When(x => !string.IsNullOrEmpty(x.Credentials));

            RuleFor(x => x.Specialization)
                .Matches(@"^[a-zA-Z0-9\s\-'.,&()]+$").WithMessage("Specialization can only contain letters, numbers, spaces, hyphens, apostrophes, commas, periods, ampersands, and parentheses")
                .When(x => !string.IsNullOrEmpty(x.Specialization));

            RuleFor(x => x.YearsOfExperience)
                .Matches(@"^[\d\s\+\-]+$").WithMessage("Years of experience can only contain numbers, spaces, plus signs, and hyphens")
                .When(x => !string.IsNullOrEmpty(x.YearsOfExperience));

            RuleFor(x => x.EmergencyContactName)
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Emergency contact name can only contain letters, spaces, hyphens, apostrophes, and periods")
                .When(x => !string.IsNullOrEmpty(x.EmergencyContactName));

            RuleFor(x => x.EmergencyContactPhone)
                .Matches(@"^[\+]?[1-9][\d]{0,15}$").WithMessage("Please enter a valid emergency contact phone number")
                .When(x => !string.IsNullOrEmpty(x.EmergencyContactPhone));

            RuleFor(x => x.EmergencyContactRelationship)
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Emergency contact relationship can only contain letters, spaces, hyphens, apostrophes, and periods")
                .When(x => !string.IsNullOrEmpty(x.EmergencyContactRelationship));
        }

        private bool BeValidRole(string role)
        {
            var validRoles = new[] { "user", "parent", "caregiver", "therapist", "adoptiveparent", "admin" };
            return validRoles.Contains(role?.ToLower());
        }

        private bool BeUniqueEmail(string email)
        {
            // This would typically check against the database
            // For now, we'll return true and handle uniqueness in the service layer
            return true;
        }
    }

    /// <summary>
    /// FluentValidation validator for user login
    /// </summary>
    public class UserLoginValidator : AbstractValidator<UserLoginDto>
    {
        public UserLoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }

    /// <summary>
    /// FluentValidation validator for user update
    /// </summary>
    public class UserUpdateValidator : AbstractValidator<UserUpdateDto>
    {
        public UserUpdateValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("First name can only contain letters, spaces, hyphens, apostrophes, and periods");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Last name can only contain letters, spaces, hyphens, apostrophes, and periods");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^[\+]?[1-9][\d]{0,15}$").WithMessage("Please enter a valid phone number")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.City)
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("City can only contain letters, spaces, hyphens, apostrophes, and periods")
                .When(x => !string.IsNullOrEmpty(x.City));

            RuleFor(x => x.State)
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("State can only contain letters, spaces, hyphens, apostrophes, and periods")
                .When(x => !string.IsNullOrEmpty(x.State));

            RuleFor(x => x.ZipCode)
                .Matches(@"^[\d\-]+$").WithMessage("Zip code can only contain numbers and hyphens")
                .When(x => !string.IsNullOrEmpty(x.ZipCode));

            RuleFor(x => x.Organization)
                .Matches(@"^[a-zA-Z0-9\s\-'\.&()]+$").WithMessage("Organization can only contain letters, numbers, spaces, hyphens, apostrophes, periods, ampersands, and parentheses")
                .When(x => !string.IsNullOrEmpty(x.Organization));

            RuleFor(x => x.Title)
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Title can only contain letters, spaces, hyphens, apostrophes, and periods")
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Credentials)
                .Matches(@"^[a-zA-Z0-9\s\-'.,&()]+$").WithMessage("Credentials can only contain letters, numbers, spaces, hyphens, apostrophes, commas, periods, ampersands, and parentheses")
                .When(x => !string.IsNullOrEmpty(x.Credentials));

            RuleFor(x => x.Specialization)
                .Matches(@"^[a-zA-Z0-9\s\-'.,&()]+$").WithMessage("Specialization can only contain letters, numbers, spaces, hyphens, apostrophes, commas, periods, ampersands, and parentheses")
                .When(x => !string.IsNullOrEmpty(x.Specialization));

            RuleFor(x => x.YearsOfExperience)
                .Matches(@"^[\d\s\+\-]+$").WithMessage("Years of experience can only contain numbers, spaces, plus signs, and hyphens")
                .When(x => !string.IsNullOrEmpty(x.YearsOfExperience));

            RuleFor(x => x.EmergencyContactName)
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Emergency contact name can only contain letters, spaces, hyphens, apostrophes, and periods")
                .When(x => !string.IsNullOrEmpty(x.EmergencyContactName));

            RuleFor(x => x.EmergencyContactPhone)
                .Matches(@"^[\+]?[1-9][\d]{0,15}$").WithMessage("Please enter a valid emergency contact phone number")
                .When(x => !string.IsNullOrEmpty(x.EmergencyContactPhone));

            RuleFor(x => x.EmergencyContactRelationship)
                .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Emergency contact relationship can only contain letters, spaces, hyphens, apostrophes, and periods")
                .When(x => !string.IsNullOrEmpty(x.EmergencyContactRelationship));

            RuleFor(x => x.Notes)
                .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }

    /// <summary>
    /// FluentValidation validator for password change
    /// </summary>
    public class PasswordChangeValidator : AbstractValidator<PasswordChangeDto>
    {
        public PasswordChangeValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(8).WithMessage("New password must be at least 8 characters long")
                .MaximumLength(100).WithMessage("New password cannot exceed 100 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
                .WithMessage("New password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")
                .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Confirm new password is required")
                .Equal(x => x.NewPassword).WithMessage("New passwords do not match");
        }
    }
}
