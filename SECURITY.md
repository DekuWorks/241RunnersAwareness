# Security Policy

## Supported Versions

We release patches for security vulnerabilities in the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 2.0.x   | :white_check_mark: |
| 1.x.x   | :x:                |

## Reporting a Vulnerability

We take security vulnerabilities seriously. If you discover a security vulnerability, please follow these steps:

### 1. **DO NOT** create a public GitHub issue

Security vulnerabilities should be reported privately to protect our users.

### 2. Email us directly

Send an email to: **security@241runnersawareness.org**

Include the following information:
- Description of the vulnerability
- Steps to reproduce the issue
- Potential impact assessment
- Any suggested fixes or mitigations

### 3. What to expect

- **Acknowledgment**: We will acknowledge receipt of your report within 48 hours
- **Initial Assessment**: We will provide an initial assessment within 5 business days
- **Regular Updates**: We will keep you informed of our progress
- **Resolution**: We aim to resolve critical vulnerabilities within 30 days

### 4. Responsible Disclosure

We follow responsible disclosure practices:
- We will not publicly disclose the vulnerability until it has been fixed
- We will credit you in our security advisories (unless you prefer to remain anonymous)
- We will not take legal action against security researchers who follow these guidelines

## Security Measures

### Authentication & Authorization
- JWT tokens with secure signing keys
- Role-based access control (RBAC)
- Session management with proper expiration
- Password hashing using BCrypt

### Data Protection
- HTTPS/TLS encryption for all communications
- SQL injection prevention through parameterized queries
- XSS protection through input sanitization
- CSRF protection for state-changing operations

### Infrastructure Security
- Azure App Service with managed certificates
- Database encryption at rest
- Regular security updates and patches
- Network security groups and firewalls

### Code Security
- Dependency vulnerability scanning
- Static code analysis
- Secure coding practices
- Regular security audits

## Security Best Practices for Contributors

### 1. Never commit secrets
- Use environment variables for sensitive configuration
- Never commit API keys, passwords, or connection strings
- Use `.env.example` files for configuration templates

### 2. Validate all inputs
- Sanitize user inputs to prevent XSS
- Validate data types and formats
- Use parameterized queries for database operations

### 3. Follow secure coding practices
- Use HTTPS for all external communications
- Implement proper error handling without information leakage
- Follow the principle of least privilege

### 4. Keep dependencies updated
- Regularly update npm/dotnet packages
- Monitor for security advisories
- Use automated dependency scanning

## Security Contacts

- **Security Team**: security@241runnersawareness.org
- **Project Maintainer**: admin@241runnersawareness.org
- **Emergency Contact**: +1-XXX-XXX-XXXX (for critical vulnerabilities only)

## Security Advisories

Security advisories will be published on our [Security Advisories](https://github.com/241RunnersAwareness/241RunnersAwareness/security/advisories) page.

## Bug Bounty Program

We currently do not have a formal bug bounty program, but we appreciate security researchers who help us improve our security posture. We may consider implementing a formal program in the future.

## Legal

By reporting a security vulnerability, you agree to:
- Not access or modify data beyond what is necessary to demonstrate the vulnerability
- Not disrupt our services or systems
- Not publicly disclose the vulnerability until we have had a chance to fix it
- Comply with all applicable laws and regulations

## Updates to this Policy

This security policy may be updated from time to time. We will notify users of significant changes through our usual communication channels.

---

**Last Updated**: January 7, 2025
**Version**: 1.0