import { Link } from 'react-router-dom';
import SEO from '../components/SEO';

const Home = () => {
  return (
    <>
      <SEO 
        title="Home"
        description="241 Runners Awareness - Supporting and protecting missing and vulnerable individuals with real-time alerts, secure data management, and advanced DNA technology."
        keywords={[
          'missing persons',
          'vulnerable individuals',
          'real-time alerts',
          'DNA tracking',
          'community safety',
          'runner awareness',
          'emergency response',
          'law enforcement',
          'missing persons cases',
          'safety technology'
        ]}
        url="/"
      />
      
      <div className="home-container">
        <section className="welcome-section">
          <h1>Welcome to 241Runners Awareness</h1>
          <p>Our mission is to support and protect missing and vulnerable individuals with real-time alerts and secure data.</p>
        </section>

        <section className="features-section">
          <h2>New Features</h2>
          <div className="features-grid">
            <div className="feature-card">
              <h3>DNA Tracking</h3>
              <p>Advanced DNA analysis and identification services for missing persons cases.</p>
              <Link to="/dna-tracking" className="feature-btn">Learn More</Link>
            </div>
          </div>
        </section>
      </div>
    </>
  );
};
  
export default Home;
  