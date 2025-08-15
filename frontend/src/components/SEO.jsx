import React from 'react';
import { Helmet } from 'react-helmet-async';

const SEO = ({ 
  title, 
  description, 
  keywords = [], 
  image = '/241-logo.jpg',
  url = '',
  type = 'website'
}) => {
  const fullUrl = url ? `https://241runnersawareness.org${url}` : 'https://241runnersawareness.org';
  const fullImageUrl = image.startsWith('http') ? image : `https://241runnersawareness.org${image}`;
  
  return (
    <Helmet>
      {/* Basic Meta Tags */}
      <title>{title ? `${title} - 241 Runners Awareness` : '241 Runners Awareness'}</title>
      <meta name="description" content={description} />
      <meta name="keywords" content={keywords.join(', ')} />
      
      {/* Open Graph Meta Tags */}
      <meta property="og:title" content={title ? `${title} - 241 Runners Awareness` : '241 Runners Awareness'} />
      <meta property="og:description" content={description} />
      <meta property="og:image" content={fullImageUrl} />
      <meta property="og:url" content={fullUrl} />
      <meta property="og:type" content={type} />
      <meta property="og:site_name" content="241 Runners Awareness" />
      
      {/* Twitter Card Meta Tags */}
      <meta name="twitter:card" content="summary_large_image" />
      <meta name="twitter:title" content={title ? `${title} - 241 Runners Awareness` : '241 Runners Awareness'} />
      <meta name="twitter:description" content={description} />
      <meta name="twitter:image" content={fullImageUrl} />
      
      {/* Additional Meta Tags */}
      <meta name="robots" content="index, follow" />
      <meta name="author" content="241 Runners Awareness" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      
      {/* Canonical URL */}
      <link rel="canonical" href={fullUrl} />
      
      {/* PWA Meta Tags */}
      <meta name="theme-color" content="#dc2626" />
      <meta name="apple-mobile-web-app-capable" content="yes" />
      <meta name="apple-mobile-web-app-status-bar-style" content="default" />
      <meta name="apple-mobile-web-app-title" content="241 Runners Awareness" />
      
      {/* Structured Data */}
      <script type="application/ld+json">
        {JSON.stringify({
          "@context": "https://schema.org",
          "@type": "NonProfit",
          "name": "241 Runners Awareness",
          "description": "Secure platform for reporting and managing missing individuals",
          "url": "https://241runnersawareness.org",
          "logo": "https://241runnersawareness.org/241-logo.jpg",
          "sameAs": [
            "https://linktr.ee/241Runners"
          ],
          "contactPoint": {
            "@type": "ContactPoint",
            "contactType": "customer service",
            "email": "info@241runnersawareness.org"
          },
          "address": {
            "@type": "PostalAddress",
            "addressCountry": "US"
          }
        })}
      </script>
    </Helmet>
  );
};

export default SEO;
