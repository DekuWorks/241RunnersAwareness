# Branch Protection Setup Guide

This guide explains how to set up comprehensive branch protection rules for the 241Runners Awareness repository to ensure code quality and security.

## 🛡️ Branch Protection Rules Overview

The branch protection rules are designed to:
- Prevent direct pushes to the main branch
- Require pull request reviews
- Enforce code quality checks
- Protect against accidental deletions
- Ensure security best practices

## 📋 Manual Setup Steps

### 1. Enable Branch Protection on GitHub

1. Go to your repository on GitHub
2. Navigate to **Settings** → **Branches**
3. Click **Add rule** or **Add branch protection rule**
4. In the **Branch name pattern** field, enter: `main`
5. Configure the following settings:

#### ✅ Required Settings

**Require a pull request before merging:**
- ✅ Require a pull request before merging
- ✅ Require approvals: `1`
- ✅ Dismiss stale pull request approvals when new commits are pushed
- ✅ Require review from code owners
- ✅ Require last push approval

**Require status checks to pass before merging:**
- ✅ Require status checks to pass before merging
- ✅ Require branches to be up to date before merging
- ✅ Status checks that are required:
  - `ci/build`
  - `ci/test`
  - `ci/lint`

**Require conversation resolution before merging:**
- ✅ Require conversation resolution before merging

**Require signed commits:**
- ❌ Require signed commits (optional - can be enabled later)

**Require linear history:**
- ❌ Require linear history (optional - can be enabled later)

**Require deployments to succeed before merging:**
- ❌ Require deployments to succeed before merging (optional)

**Lock branch:**
- ❌ Lock branch (optional - prevents all changes)

#### 🚫 Restrictions

**Restrict pushes that create files:**
- ❌ Do not allow bypassing the above settings
- ❌ Allow force pushes
- ❌ Allow deletions

**Restrict pushes that create files:**
- ✅ Allow fork syncing

### 2. Enable Security Features

Navigate to **Settings** → **Security** and enable:

#### 🔒 Security & Analysis
- ✅ **Dependency graph**: Enable
- ✅ **Dependabot alerts**: Enable
- ✅ **Dependabot security updates**: Enable
- ✅ **Secret scanning**: Enable
- ✅ **Secret scanning push protection**: Enable
- ❌ **Code scanning**: Enable (optional - requires setup)
- ❌ **Advanced security**: Enable (optional - requires GitHub Enterprise)

### 3. Configure Repository Rules

Navigate to **Settings** → **General** → **Repository rules**:

#### 📝 Pull Request Rules
- ✅ Require pull request reviews before merging
- ✅ Require review from code owners
- ✅ Dismiss stale reviews when new commits are pushed
- ✅ Require review from code owners
- ✅ Require last push approval

#### ✅ Status Checks
- ✅ Require status checks to pass before merging
- ✅ Require branches to be up to date before merging
- ✅ Strict status checks

#### 🔐 Commit Signing
- ❌ Require signed commits (optional)

#### 📈 History
- ❌ Require linear history (optional)

#### 🚫 Push Restrictions
- ❌ Allow force pushes
- ❌ Allow deletions

### 4. Set Up Code Owners

The `CODEOWNERS` file is already configured to automatically request reviews from `@DekuWorks` for all changes.

### 5. Configure Workflow Permissions

Navigate to **Settings** → **Actions** → **General**:

#### 🔧 Workflow permissions
- ✅ **Allow GitHub Actions to create and approve pull requests**
- ✅ **Read and write permissions**
- ✅ **Allow GitHub Actions to create and approve pull requests**

#### 🔐 Environment protection rules
- ✅ **Required reviewers**: Add yourself as a reviewer
- ✅ **Wait timer**: `0` minutes
- ✅ **Deployment branches**: Restrict to protected branches

## 🚀 Automated Setup

The repository includes GitHub Actions workflows that will automatically:

### Branch Protection Checks
- ✅ Security and code quality checks
- ✅ Build and test validation
- ✅ Linting and code style enforcement
- ✅ Documentation validation
- ✅ Deployment readiness checks

### Required Status Checks
The following status checks must pass before merging:
- `security-checks`
- `build-tests`
- `linting`
- `documentation`
- `deployment-check`

## 📊 Monitoring and Maintenance

### Regular Tasks
1. **Review failed checks** - Check GitHub Actions for any failed workflows
2. **Update dependencies** - Regularly update npm packages and .NET packages
3. **Review security alerts** - Monitor Dependabot alerts and secret scanning results
4. **Update branch protection rules** - Adjust rules as the project evolves

### Troubleshooting

#### Common Issues

**"Branch is not up to date"**
- Solution: Rebase your branch with main before creating a PR

**"Required status checks are not passing"**
- Solution: Check the GitHub Actions tab and fix any failing tests

**"Review required from code owners"**
- Solution: Ensure @DekuWorks reviews the pull request

**"Conversation resolution required"**
- Solution: Address all review comments and mark them as resolved

#### Emergency Override

In case of emergency, repository administrators can:
1. Temporarily disable branch protection
2. Force push to main (not recommended)
3. Merge without reviews (use sparingly)

## 🔄 Workflow

### Standard Development Process

1. **Create a feature branch** from main
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** and commit them
   ```bash
   git add .
   git commit -m "Add your feature description"
   ```

3. **Push to your branch**
   ```bash
   git push origin feature/your-feature-name
   ```

4. **Create a pull request** on GitHub
   - Title: Clear description of changes
   - Description: Detailed explanation of what was changed and why
   - Assign reviewers if needed

5. **Wait for checks to pass**
   - All status checks must be green
   - At least one approval required
   - All conversations must be resolved

6. **Merge the pull request**
   - Use "Squash and merge" for clean history
   - Delete the feature branch after merging

### Emergency Hotfix Process

1. **Create a hotfix branch** from main
   ```bash
   git checkout -b hotfix/emergency-fix
   ```

2. **Make minimal changes** to fix the issue
3. **Test thoroughly** before creating PR
4. **Create pull request** with "HOTFIX" in the title
5. **Request expedited review** from code owners
6. **Merge immediately** after approval

## 📞 Support

If you encounter issues with branch protection:

1. **Check the GitHub Actions** tab for detailed error messages
2. **Review the branch protection settings** in repository settings
3. **Contact the repository administrator** (@DekuWorks) for assistance
4. **Check this documentation** for troubleshooting steps

## 🔗 Additional Resources

- [GitHub Branch Protection Documentation](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/defining-the-mergeability-of-pull-requests/about-protected-branches)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [CODEOWNERS Documentation](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners)
- [Security Best Practices](https://docs.github.com/en/code-security)

---

**Last Updated**: January 2025  
**Maintained by**: @DekuWorks
