import React from 'react';
import { Link } from 'react-router-dom';
import SEO from '../components/SEO';

const About = () => {
  return (
    <>
      <SEO 
        title="About Us"
        description="Learn about 241 Runners Awareness, our mission to honor Israel Thomas, and our team dedicated to supporting missing and vulnerable individuals through technology and community engagement."
        keywords={[
          '241 runners awareness',
          'Israel Thomas',
          'missing persons',
          'community safety',
          'runner awareness',
          'team members',
          'mission statement',
          'vulnerable individuals',
          'technology platform',
          'emergency response'
        ]}
        url="/about"
      />
      
      <div className="about-content">
        <h2 className="form-title" id="about-heading">About 241 Runners Awareness</h2>

        <div className="mission-statement">
          <h3>Our Mission</h3>
          <p>241 Runners Awareness is dedicated to honoring the memory of Israel Thomas and supporting and protecting missing and vulnerable individuals through real-time alerts, secure data management, and community engagement. We believe that every person deserves to be safe and that technology can be a powerful tool in preventing tragedies and bringing families closure.</p>
        </div>

        <div className="israel-legacy-section">
          <h3>Israel's Legacy</h3>
          <p>In memory of Israel Thomas, who passed away at <strong>2:41 AM</strong>, our organization works tirelessly to prevent similar tragedies and support families affected by missing persons cases. Israel's memory drives our mission to create safer communities and provide hope to families in crisis.</p>
        </div>

        <h3>Who We Are</h3>
        <p>Founded by passionate runners and community advocates, 241 Runners Awareness emerged from a deep commitment to public safety and community well-being. Our name "241" represents <strong>2:41 AM</strong>, the time when Israel Thomas passed away, symbolizing our mission to honor his memory and prevent similar tragedies through awareness, technology, and community support.</p>
        
        <div className="who-we-are-images">
          <div className="team-member">
            <img src="/docs/assets/lisa_thomas_241.jpg" alt="Lisa Thomas - Founder of 241 Runners Awareness" loading="lazy" />
            <div className="image-caption">Lisa Thomas - Founder</div>
          </div>
          <div className="team-member">
            <img src="/docs/assets/7B0987ED-8681-4E9B-AE36-C54B7E59C0B6.JPG" alt="Marcus Brown - Lead Front End Developer" loading="lazy" />
            <div className="image-caption">Marcus Brown - Lead Front End Developer</div>
          </div>
          <div className="team-member">
            <img src="/docs/assets/DSC00405.JPG" alt="Daniel Carey - Full Stack Developer" loading="lazy" />
            <div className="image-caption">Daniel Carey - Full Stack Developer</div>
          </div>
          <div className="team-member">
            <img src="/docs/assets/IMG_1468.PNG" alt="Tina Matthews - Program Director" loading="lazy" />
            <div className="image-caption">Tina Matthews - Program Director</div>
          </div>
          <div className="team-member">
            <img src="/docs/assets/IMG_0834.JPG" alt="Ralph Frank - Event Coordinator" loading="lazy" />
            <div className="image-caption">Ralph Frank - Event Coordinator</div>
          </div>
          <div className="team-member">
            <img src="/docs/assets/123_1.JPEG" alt="Arquelle Gilder - Real Estate Broker / Sponsor" loading="lazy" />
            <div className="image-caption">Arquelle Gilder - Real Estate Broker / Sponsor</div>
          </div>
        </div>

        <div className="what-we-do-section">
          <h3>What We Do</h3>
          <p>Our platform provides a comprehensive solution for tracking and protecting vulnerable individuals through:</p>
          <ul>
            <li><strong>Real-time Alerts:</strong> Instant notifications when vulnerable individuals are reported missing or in distress</li>
            <li><strong>Secure Data Management:</strong> Protected storage of critical information for emergency situations</li>
            <li><strong>Community Integration:</strong> Connecting runners, law enforcement, and community members in safety efforts</li>
            <li><strong>Emergency Response Coordination:</strong> Streamlined communication between all parties during critical situations</li>
            <li><strong>DNA Technology:</strong> Advanced DNA collection and identification for missing persons cases</li>
            <li><strong>Fundraising Support:</strong> Premium merchandise sales supporting our mission and technology</li>
          </ul>
        </div>

        <div className="technology-section">
          <h3>Our Technology</h3>
          <p>We leverage cutting-edge technology to create a seamless experience for users while maintaining the highest standards of security and privacy. Our platform is designed to be accessible, reliable, and user-friendly for everyone from individual runners to law enforcement agencies.</p>
        </div>

        <div className="get-involved-section">
          <h3>Get Involved</h3>
          <p>Whether you're a runner, community member, or organization interested in supporting our mission, there are many ways to get involved:</p>
          <ul>
            <li>Sign up for our platform to receive alerts and updates</li>
            <li>Volunteer with our community outreach programs</li>
            <li>Support our mission through donations</li>
            <li>Follow us on social media to stay connected</li>
          </ul>
          <p>Together, we can create safer communities and protect those who need it most.</p>
        </div>
      </div>
    </>
  );
};

export default About;
