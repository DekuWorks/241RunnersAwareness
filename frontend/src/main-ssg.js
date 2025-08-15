/**
 * ============================================
 * 241 RUNNERS AWARENESS - SSG ENTRY POINT
 * ============================================
 * 
 * This is the SSG entry point for static site generation.
 * It sets up the application for pre-rendering static pages.
 */

import { ViteSSG } from 'vite-ssg'
import App from './App.jsx'
import './index.css'

// Import all pages for SSG
import Home from './pages/Home.jsx'
import About from './pages/About.jsx'
import Cases from './pages/Cases.jsx'
import Map from './pages/Map.jsx'
import Shop from './pages/Shop.jsx'
import DNATracking from './pages/DNATracking.jsx'
import Privacy from './pages/Privacy.jsx'
import Terms from './pages/Terms.jsx'
import Offline from './pages/Offline.jsx'
import RegisterPage from './pages/RegisterPage.jsx'
import LoginPage from './pages/LoginPage.jsx'
import NotFound from './pages/NotFound.jsx'

// Redux store
import { store } from './store'

// Google OAuth
import { GoogleOAuthProvider } from '@react-oauth/google'
import { GOOGLE_CLIENT_ID } from './config/api'

// Routes for SSG
const routes = [
  { path: '/', component: Home },
  { path: '/about', component: About },
  { path: '/cases', component: Cases },
  { path: '/map', component: Map },
  { path: '/shop', component: Shop },
  { path: '/dna-tracking', component: DNATracking },
  { path: '/privacy', component: Privacy },
  { path: '/terms', component: Terms },
  { path: '/offline', component: Offline },
  { path: '/register', component: RegisterPage },
  { path: '/login', component: LoginPage },
  { path: '/:pathMatch(.*)*', component: NotFound }
]

// Create SSG app
export const createApp = ViteSSG(
  App,
  { routes },
  async (ctx) => {
    // Install plugins
    ctx.app.use(store)
    
    // Wrap with providers
    ctx.app.component('GoogleOAuthProvider', GoogleOAuthProvider)
    ctx.app.provide('googleClientId', GOOGLE_CLIENT_ID)
  }
)
