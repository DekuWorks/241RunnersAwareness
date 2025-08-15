/**
 * Sitemap Generator for 241 Runners Awareness
 * Generates sitemap.xml for better SEO
 */

import { SitemapStream, streamToPromise } from 'sitemap'
import { createWriteStream } from 'fs'
import { resolve } from 'path'

const baseUrl = 'https://241runnersawareness.org'

// Define all routes for the sitemap
const routes = [
  {
    url: '/',
    changefreq: 'daily',
    priority: 1.0,
    lastmod: new Date().toISOString()
  },
  {
    url: '/about',
    changefreq: 'monthly',
    priority: 0.8,
    lastmod: new Date().toISOString()
  },
  {
    url: '/cases',
    changefreq: 'daily',
    priority: 0.9,
    lastmod: new Date().toISOString()
  },
  {
    url: '/map',
    changefreq: 'daily',
    priority: 0.8,
    lastmod: new Date().toISOString()
  },
  {
    url: '/shop',
    changefreq: 'weekly',
    priority: 0.7,
    lastmod: new Date().toISOString()
  },
  {
    url: '/dna-tracking',
    changefreq: 'monthly',
    priority: 0.6,
    lastmod: new Date().toISOString()
  },
  {
    url: '/privacy',
    changefreq: 'yearly',
    priority: 0.3,
    lastmod: new Date().toISOString()
  },
  {
    url: '/terms',
    changefreq: 'yearly',
    priority: 0.3,
    lastmod: new Date().toISOString()
  },
  {
    url: '/register',
    changefreq: 'monthly',
    priority: 0.5,
    lastmod: new Date().toISOString()
  },
  {
    url: '/login',
    changefreq: 'monthly',
    priority: 0.5,
    lastmod: new Date().toISOString()
  }
]

async function generateSitemap() {
  try {
    console.log('Generating sitemap...')
    
    // Create sitemap stream
    const sitemap = new SitemapStream({ hostname: baseUrl })
    
    // Add routes to sitemap
    routes.forEach(route => {
      sitemap.write(route)
    })
    
    sitemap.end()
    
    // Convert to string
    const sitemapString = await streamToPromise(sitemap)
    
    // Write to file
    const outputPath = resolve(__dirname, '../dist/sitemap.xml')
    const writeStream = createWriteStream(outputPath)
    writeStream.write(sitemapString.toString())
    writeStream.end()
    
    console.log(`Sitemap generated at: ${outputPath}`)
  } catch (error) {
    console.error('Error generating sitemap:', error)
  }
}

generateSitemap()
