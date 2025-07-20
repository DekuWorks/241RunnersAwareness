import React, { useState } from 'react';
import { toast } from 'react-hot-toast';

const DNATracking = () => {
  const [dnaForm, setDnaForm] = useState({
    individualId: '',
    sampleType: '',
    labReference: '',
    dnaSequence: '',
    notes: ''
  });

  const [reportIndividualId, setReportIndividualId] = useState('');
  const [dnaReport, setDnaReport] = useState('');

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setDnaForm(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const submitDNASample = () => {
    const { individualId, sampleType, labReference, dnaSequence } = dnaForm;

    if (!individualId || !sampleType || !labReference || !dnaSequence) {
      toast.error('Please fill in all required fields');
      return;
    }

    // Validate DNA sequence
    const validChars = /^[ATCGatcg]+$/;
    if (!validChars.test(dnaSequence)) {
      toast.error('DNA sequence must contain only A, T, C, G characters');
      return;
    }

    if (dnaSequence.length < 10) {
      toast.error('DNA sequence must be at least 10 characters long');
      return;
    }

    // Mock submission
    toast.success('DNA sample submitted successfully!');
    
    // Clear form
    setDnaForm({
      individualId: '',
      sampleType: '',
      labReference: '',
      dnaSequence: '',
      notes: ''
    });
  };

  const generateDNAReport = () => {
    if (!reportIndividualId) {
      toast.error('Please enter an Individual ID');
      return;
    }

    // Mock report generation
    const report = `DNA Analysis Report
==================

Individual ID: ${reportIndividualId}
Report Date: ${new Date().toLocaleString()}
Sample Type: Buccal Swab
Lab Reference: LAB-${reportIndividualId.padStart(6, '0')}
Sample Date: ${new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toLocaleDateString()}

DNA Sequence Analysis:
- Sequence Length: 1,247 base pairs
- GC Content: 42.3%
- Sequence Quality: High

Genetic Markers Identified:
- STR Markers: D3S1358, vWA, FGA, D8S1179, D21S11, D18S51, D5S818, D13S317, D7S820, D16S539, TH01, TPOX, CSF1PO
- SNP Markers: rs53576, rs1815739, rs1800497, rs1801133
- Haplotype: H1a1a

Comparison Results:
- NAMUS Database: No matches found
- CODIS Database: No matches found
- Local Database: No matches found

Recommendations:
- Submit to NAMUS database for national comparison
- Submit to CODIS database for criminal database comparison
- Schedule follow-up testing in 6 months
- Consider additional family member sampling

This report is generated for law enforcement and missing persons investigation purposes.`;

    setDnaReport(report);
    toast.success('DNA report generated successfully!');
  };

  return (
    <div className="dna-container">
      {/* Hero Section */}
      <div className="hero-section">
        <h1>Advanced DNA Technology</h1>
        <p>State-of-the-art DNA tracking and identification system for missing persons cases. Every sample helps bring families closure.</p>
        <div className="hero-buttons">
          <button onClick={() => document.getElementById('stats').scrollIntoView({ behavior: 'smooth' })}>
            📊 Statistics
          </button>
          <button onClick={() => document.getElementById('features').scrollIntoView({ behavior: 'smooth' })}>
            🔬 Features
          </button>
          <button onClick={() => document.getElementById('databases').scrollIntoView({ behavior: 'smooth' })}>
            🗄️ Databases
          </button>
        </div>
      </div>

      {/* Statistics Section */}
      <div className="stats-section" id="stats">
        <h2>DNA Collection Statistics</h2>
        <div className="stats-grid">
          <div className="stat-card">
            <span className="stat-number">1,250</span>
            <span className="stat-label">Total Samples Collected</span>
          </div>
          <div className="stat-card">
            <span className="stat-number">45</span>
            <span className="stat-label">Samples This Month</span>
          </div>
          <div className="stat-card">
            <span className="stat-number">98.5%</span>
            <span className="stat-label">Success Rate</span>
          </div>
          <div className="stat-card">
            <span className="stat-number">3.2 days</span>
            <span className="stat-label">Average Processing Time</span>
          </div>
          <div className="stat-card">
            <span className="stat-number">156</span>
            <span className="stat-label">Active Cases</span>
          </div>
          <div className="stat-card">
            <span className="stat-number">89</span>
            <span className="stat-label">Resolved Cases</span>
          </div>
        </div>
      </div>

      {/* DNA Features */}
      <div className="dna-features" id="features">
        <h2>DNA Technology Features</h2>
        <div className="features-grid">
          <div className="feature-card">
            <div className="feature-icon">🧬</div>
            <div className="feature-title">DNA Sample Collection</div>
            <div className="feature-description">
              Secure collection of DNA samples using buccal swabs, blood samples, and hair follicles. 
              All samples are encrypted and stored with military-grade security protocols.
            </div>
          </div>

          <div className="feature-card">
            <div className="feature-icon">🔍</div>
            <div className="feature-title">Advanced Analysis</div>
            <div className="feature-description">
              Sophisticated DNA analysis including STR markers, SNP analysis, and haplotype determination. 
              Results are compared against national databases for matches.
            </div>
          </div>

          <div className="feature-card">
            <div className="feature-icon">🗄️</div>
            <div className="feature-title">Database Integration</div>
            <div className="feature-description">
              Automatic integration with NAMUS, CODIS, and local law enforcement databases. 
              Real-time matching and notification systems.
            </div>
          </div>

          <div className="feature-card">
            <div className="feature-icon">📊</div>
            <div className="feature-title">Comprehensive Reports</div>
            <div className="feature-description">
              Detailed DNA analysis reports including genetic markers, quality assessment, 
              and comparison results with recommendations for further testing.
            </div>
          </div>

          <div className="feature-card">
            <div className="feature-icon">🔐</div>
            <div className="feature-title">Secure Storage</div>
            <div className="feature-description">
              Encrypted DNA sequence storage with HIPAA compliance and law enforcement 
              security standards. Access controlled by role-based permissions.
            </div>
          </div>

          <div className="feature-card">
            <div className="feature-icon">🚨</div>
            <div className="feature-title">Real-Time Alerts</div>
            <div className="feature-description">
              Instant notifications when DNA matches are found in national databases. 
              Automatic alerts to law enforcement and family members.
            </div>
          </div>
        </div>
      </div>

      {/* DNA Sample Form */}
      <div className="dna-form-section">
        <h2>Submit DNA Sample</h2>
        <div className="form-grid">
          <div>
            <div className="form-group">
              <label className="form-label">Individual ID</label>
              <input 
                type="text" 
                className="form-input" 
                name="individualId"
                value={dnaForm.individualId}
                onChange={handleInputChange}
                placeholder="Enter individual ID"
              />
            </div>
            <div className="form-group">
              <label className="form-label">Sample Type</label>
              <select 
                className="form-input" 
                name="sampleType"
                value={dnaForm.sampleType}
                onChange={handleInputChange}
              >
                <option value="">Select sample type</option>
                <option value="Buccal Swab">Buccal Swab</option>
                <option value="Blood Sample">Blood Sample</option>
                <option value="Hair Follicle">Hair Follicle</option>
                <option value="Saliva">Saliva</option>
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">Lab Reference</label>
              <input 
                type="text" 
                className="form-input" 
                name="labReference"
                value={dnaForm.labReference}
                onChange={handleInputChange}
                placeholder="Lab reference number"
              />
            </div>
          </div>
          <div>
            <div className="form-group">
              <label className="form-label">DNA Sequence</label>
              <textarea 
                className="form-textarea" 
                name="dnaSequence"
                value={dnaForm.dnaSequence}
                onChange={handleInputChange}
                placeholder="Enter DNA sequence (A, T, C, G only)"
              />
            </div>
            <div className="form-group">
              <label className="form-label">Notes</label>
              <textarea 
                className="form-textarea" 
                name="notes"
                value={dnaForm.notes}
                onChange={handleInputChange}
                placeholder="Additional notes about the sample"
              />
            </div>
            <button className="submit-btn" onClick={submitDNASample}>
              Submit DNA Sample
            </button>
          </div>
        </div>
      </div>

      {/* Database Integration */}
      <div className="database-section" id="databases">
        <h2>National Database Integration</h2>
        <div className="database-grid">
          <div className="database-card">
            <h3>NAMUS Database</h3>
            <p>National Missing and Unidentified Persons System integration for nationwide DNA matching</p>
            <div className="database-stats">
              <div className="db-stat">
                <span className="db-stat-number">23</span>
                <span className="db-stat-label">Matches Found</span>
              </div>
              <div className="db-stat">
                <span className="db-stat-number">1,150</span>
                <span className="db-stat-label">Samples Uploaded</span>
              </div>
            </div>
          </div>

          <div className="database-card">
            <h3>CODIS Database</h3>
            <p>Combined DNA Index System for criminal database comparison and identification</p>
            <div className="database-stats">
              <div className="db-stat">
                <span className="db-stat-number">8</span>
                <span className="db-stat-label">Matches Found</span>
              </div>
              <div className="db-stat">
                <span className="db-stat-number">890</span>
                <span className="db-stat-label">Samples Uploaded</span>
              </div>
            </div>
          </div>

          <div className="database-card">
            <h3>Local Database</h3>
            <p>Houston area missing persons database for local law enforcement collaboration</p>
            <div className="database-stats">
              <div className="db-stat">
                <span className="db-stat-number">156</span>
                <span className="db-stat-label">Active Cases</span>
              </div>
              <div className="db-stat">
                <span className="db-stat-number">89</span>
                <span className="db-stat-label">Resolved Cases</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Lab Partnerships */}
      <div className="labs-section">
        <h2>Partner Laboratories</h2>
        <div className="labs-grid">
          <div className="lab-card">
            <div className="lab-logo">🏥</div>
            <h3>Houston Forensic Science Center</h3>
            <p>Primary forensic DNA analysis laboratory</p>
            <span className="lab-badge primary">Primary Partner</span>
          </div>

          <div className="lab-card">
            <div className="lab-logo">🔬</div>
            <h3>Texas Department of Public Safety Crime Lab</h3>
            <p>State crime laboratory for DNA analysis</p>
            <span className="lab-badge state">State Lab</span>
          </div>

          <div className="lab-card">
            <div className="lab-logo">🕵️</div>
            <h3>FBI DNA Analysis Unit</h3>
            <p>Federal Bureau of Investigation DNA analysis</p>
            <span className="lab-badge federal">Federal</span>
          </div>

          <div className="lab-card">
            <div className="lab-logo">🎓</div>
            <h3>University of Texas DNA Lab</h3>
            <p>Academic research and advanced DNA analysis</p>
            <span className="lab-badge research">Research</span>
          </div>

          <div className="lab-card">
            <div className="lab-logo">🧪</div>
            <h3>Bode Technology Group</h3>
            <p>Private DNA analysis and forensic services</p>
            <span className="lab-badge private">Private</span>
          </div>

          <div className="lab-card">
            <div className="lab-logo">🔬</div>
            <h3>DNA Diagnostics Center</h3>
            <p>Specialized DNA testing and analysis</p>
            <span className="lab-badge specialized">Specialized</span>
          </div>
        </div>
      </div>

      {/* DNA Report Generator */}
      <div className="report-section">
        <h2>DNA Report Generator</h2>
        <div className="form-grid">
          <div>
            <div className="form-group">
              <label className="form-label">Individual ID for Report</label>
              <input 
                type="text" 
                className="form-input" 
                value={reportIndividualId}
                onChange={(e) => setReportIndividualId(e.target.value)}
                placeholder="Enter individual ID"
              />
            </div>
            <button className="submit-btn" onClick={generateDNAReport}>
              Generate DNA Report
            </button>
          </div>
          <div>
            <div className="report-preview">
              {dnaReport || 'Enter an Individual ID and click "Generate DNA Report" to view a sample report.'}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default DNATracking; 