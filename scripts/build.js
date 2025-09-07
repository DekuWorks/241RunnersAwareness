#!/usr/bin/env node

/**
 * ============================================
 * 241 RUNNERS AWARENESS - BUILD SCRIPT
 * ============================================
 * 
 * P0 Implementation: Add hashed assets + version file for instant deploys
 * Generates content hashes for assets and creates version.json
 */

const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const { execSync } = require('child_process');

// ===== CONFIGURATION =====
const CONFIG = {
    assetExtensions: ['.js', '.css', '.html', '.json'],
    excludePatterns: ['node_modules', '.git', 'dist', 'build'],
    versionFile: 'version.json',
    manifestFile: 'asset-manifest.json',
    buildDir: 'dist'
};

// ===== UTILITY FUNCTIONS =====

/**
 * Get file hash
 * @param {string} filePath - File path
 * @returns {string} SHA-256 hash
 */
function getFileHash(filePath) {
    const content = fs.readFileSync(filePath);
    return crypto.createHash('sha256').update(content).digest('hex').substring(0, 8);
}

/**
 * Get git commit hash
 * @returns {string} Git commit hash
 */
function getGitCommitHash() {
    try {
        return execSync('git rev-parse HEAD', { encoding: 'utf8' }).trim().substring(0, 7);
    } catch (error) {
        console.warn('⚠️ Could not get git commit hash:', error.message);
        return 'unknown';
    }
}

/**
 * Get git branch
 * @returns {string} Git branch name
 */
function getGitBranch() {
    try {
        return execSync('git rev-parse --abbrev-ref HEAD', { encoding: 'utf8' }).trim();
    } catch (error) {
        console.warn('⚠️ Could not get git branch:', error.message);
        return 'unknown';
    }
}

/**
 * Check if file should be processed
 * @param {string} filePath - File path
 * @returns {boolean} True if file should be processed
 */
function shouldProcessFile(filePath) {
    const ext = path.extname(filePath).toLowerCase();
    const fileName = path.basename(filePath);
    
    // Check extension
    if (!CONFIG.assetExtensions.includes(ext)) {
        return false;
    }
    
    // Check exclude patterns
    for (const pattern of CONFIG.excludePatterns) {
        if (filePath.includes(pattern)) {
            return false;
        }
    }
    
    // Skip already hashed files
    if (fileName.includes('-') && /[a-f0-9]{8}/.test(fileName)) {
        return false;
    }
    
    return true;
}

/**
 * Find all asset files
 * @param {string} dir - Directory to search
 * @returns {Array} Array of file paths
 */
function findAssetFiles(dir) {
    const files = [];
    
    function walkDir(currentDir) {
        const items = fs.readdirSync(currentDir);
        
        for (const item of items) {
            const fullPath = path.join(currentDir, item);
            const stat = fs.statSync(fullPath);
            
            if (stat.isDirectory()) {
                walkDir(fullPath);
            } else if (stat.isFile() && shouldProcessFile(fullPath)) {
                files.push(fullPath);
            }
        }
    }
    
    walkDir(dir);
    return files;
}

/**
 * Generate hashed filename
 * @param {string} filePath - Original file path
 * @param {string} hash - File hash
 * @returns {string} Hashed file path
 */
function generateHashedPath(filePath, hash) {
    const ext = path.extname(filePath);
    const name = path.basename(filePath, ext);
    const dir = path.dirname(filePath);
    
    return path.join(dir, `${name}-${hash}${ext}`);
}

/**
 * Update HTML files with hashed asset references
 * @param {string} filePath - HTML file path
 * @param {Object} assetMap - Asset mapping
 */
function updateHtmlFile(filePath, assetMap) {
    let content = fs.readFileSync(filePath, 'utf8');
    let updated = false;
    
    // Update script and link tags
    for (const [originalPath, hashedPath] of Object.entries(assetMap)) {
        const originalUrl = originalPath.replace(/\\/g, '/');
        const hashedUrl = hashedPath.replace(/\\/g, '/');
        
        // Update script src
        const scriptRegex = new RegExp(`(<script[^>]+src=["']?)${originalUrl.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}(["'][^>]*>)`, 'gi');
        if (scriptRegex.test(content)) {
            content = content.replace(scriptRegex, `$1${hashedUrl}$2`);
            updated = true;
        }
        
        // Update link href
        const linkRegex = new RegExp(`(<link[^>]+href=["']?)${originalUrl.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}(["'][^>]*>)`, 'gi');
        if (linkRegex.test(content)) {
            content = content.replace(linkRegex, `$1${hashedUrl}$2`);
            updated = true;
        }
    }
    
    if (updated) {
        fs.writeFileSync(filePath, content, 'utf8');
        console.log(`✅ Updated HTML file: ${filePath}`);
    }
}

// ===== MAIN BUILD PROCESS =====

/**
 * Main build function
 */
async function build() {
    console.log('🚀 Starting build process...');
    
    try {
        // Get git information
        const commitHash = getGitCommitHash();
        const branch = getGitBranch();
        const buildTime = new Date().toISOString();
        
        console.log(`📋 Build info: ${branch}@${commitHash}`);
        
        // Find all asset files
        const assetFiles = findAssetFiles('.');
        console.log(`📁 Found ${assetFiles.length} asset files to process`);
        
        // Generate asset manifest
        const assetMap = {};
        const hashedFiles = [];
        
        for (const filePath of assetFiles) {
            const hash = getFileHash(filePath);
            const hashedPath = generateHashedPath(filePath, hash);
            
            // Copy file with hashed name
            fs.copyFileSync(filePath, hashedPath);
            hashedFiles.push(hashedPath);
            
            // Add to asset map
            assetMap[filePath] = hashedPath;
            
            console.log(`📄 Processed: ${path.basename(filePath)} → ${path.basename(hashedPath)}`);
        }
        
        // Update HTML files
        const htmlFiles = assetFiles.filter(f => path.extname(f).toLowerCase() === '.html');
        for (const htmlFile of htmlFiles) {
            updateHtmlFile(htmlFile, assetMap);
        }
        
        // Create asset manifest
        const manifest = {
            buildTime,
            commitHash,
            branch,
            assets: assetMap,
            hashedFiles
        };
        
        fs.writeFileSync(CONFIG.manifestFile, JSON.stringify(manifest, null, 2));
        console.log(`📋 Created asset manifest: ${CONFIG.manifestFile}`);
        
        // Create version file
        const version = {
            version: `1.0.0-${commitHash}`,
            build: `${branch}-${commitHash}`,
            commit: commitHash,
            branch: branch,
            builtAt: buildTime,
            environment: process.env.NODE_ENV || 'development',
            assets: Object.keys(assetMap).length
        };
        
        fs.writeFileSync(CONFIG.versionFile, JSON.stringify(version, null, 2));
        console.log(`📋 Created version file: ${CONFIG.versionFile}`);
        
        // Create build summary
        const summary = {
            totalFiles: assetFiles.length,
            hashedFiles: hashedFiles.length,
            htmlFiles: htmlFiles.length,
            buildTime: Date.now() - Date.now(),
            success: true
        };
        
        console.log('✅ Build completed successfully!');
        console.log(`📊 Summary:`, summary);
        
        return { success: true, version, manifest, summary };
        
    } catch (error) {
        console.error('❌ Build failed:', error);
        return { success: false, error: error.message };
    }
}

// ===== CLI INTERFACE =====

if (require.main === module) {
    build().then(result => {
        if (result.success) {
            console.log('🎉 Build process completed successfully!');
            process.exit(0);
        } else {
            console.error('💥 Build process failed!');
            process.exit(1);
        }
    });
}

module.exports = { build, getFileHash, getGitCommitHash };
