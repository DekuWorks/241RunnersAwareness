# Data Sources Documentation

This document provides information about the data sources used by the 241 Runners Awareness system for tracking missing persons cases in the Houston area.

## Primary Data Sources

### 1. NamUs (National Missing and Unidentified Persons System)

**Website**: [https://namus.nij.ojp.gov/](https://namus.nij.ojp.gov/)

**Description**: NamUs is a national centralized repository and resource center for missing persons and unidentified decedent records. It is funded and administered by the National Institute of Justice (NIJ) and provides free access to case information.

**Data Available**:
- Missing person case details
- Physical descriptions
- Last known location
- Contact information for investigating agencies
- Case status updates

**Export Process**:
1. Navigate to [NamUs Advanced Search](https://namus.nij.ojp.gov/search/missing-persons)
2. Use filters to search for cases in Texas
3. Set location filters for Houston/Harris County
4. Export results to CSV format
5. Upload CSV to the admin dashboard

**Required CSV Headers**:
- Case Number
- Full Name
- Sex
- Age at Missing
- Date Missing
- City
- County
- State
- Agency

### 2. Texas Department of Public Safety (TxDPS)

**Website**: [https://www.dps.texas.gov/](https://www.dps.texas.gov/)

**Missing Persons Bulletin**: [https://www.dps.texas.gov/missingpersons/](https://www.dps.texas.gov/missingpersons/)

**Description**: TxDPS maintains a comprehensive database of missing persons in Texas and provides bulletins for public awareness and law enforcement coordination.

**Data Available**:
- Current missing persons cases
- Case resolution updates
- Law enforcement contact information
- Case status changes

**Verification Process**:
1. Check TxDPS missing persons bulletin for case updates
2. Contact local law enforcement agencies
3. Verify case resolution status
4. Update case status in the system

### 3. Harris County Institute of Forensic Sciences (HCIFS)

**Website**: [https://ifs.harriscountytx.gov/](https://ifs.harriscountytx.gov/)

**Description**: HCIFS provides forensic services including DNA analysis, identification services, and support for missing persons investigations in Harris County.

**Services Available**:
- DNA analysis and comparison
- Forensic identification
- Expert testimony
- Case consultation

**Contact Information**:
- Phone: (713) 796-9292
- Email: info@ifs.harriscountytx.gov
- Address: 1861 Old Spanish Trail, Houston, TX 77054

## Secondary Data Sources

### 4. Houston Police Department (HPD)

**Website**: [https://www.houstonpolice.org/](https://www.houstonpolice.org/)

**Missing Persons Unit**: Contact HPD Missing Persons Unit for case-specific information and updates.

**Contact Information**:
- Phone: (713) 308-3100
- Email: missingpersons@houstonpolice.org

### 5. Texas Missing Persons Clearinghouse

**Website**: [https://www.dps.texas.gov/missingpersons/](https://www.dps.texas.gov/missingpersons/)

**Description**: State-level clearinghouse for missing persons information and resources.

### 6. National Center for Missing & Exploited Children (NCMEC)

**Website**: [https://www.missingkids.org/](https://www.missingkids.org/)

**Description**: National resource center for missing and exploited children cases.

## Data Import Workflow

### Weekly NamUs Import Process

1. **Export from NamUs**:
   - Log into NamUs account
   - Perform advanced search for Texas cases
   - Filter by Houston/Harris County location
   - Export results to CSV format

2. **Upload to System**:
   - Access admin dashboard
   - Navigate to NamUs Data Management section
   - Upload CSV file
   - Review import results and errors

3. **Data Processing**:
   - System validates CSV format and required fields
   - Filters cases to Houston area only
   - Updates existing cases or creates new ones
   - Marks cases as "resolved_pending_verify" if not in latest export

### Case Verification Process

1. **Check TxDPS Bulletin**:
   - Search for case by name and date
   - Look for status updates or resolution notices
   - Note any new information

2. **Contact Local Agencies**:
   - Reach out to investigating agency
   - Request case status update
   - Verify any resolution information

3. **Update System**:
   - Use admin interface to update case status
   - Add verification source and notes
   - Mark case as resolved with appropriate status

## Data Quality Standards

### Required Information
- Valid NamUs case number
- Complete name information
- Accurate location data
- Current case status

### Data Validation
- CSV header validation
- Required field checking
- Location filtering (Texas/Houston area only)
- Date format validation

### Error Handling
- Import error logging
- Partial import support
- Data validation feedback
- Retry mechanisms

## Privacy and Security

### Data Protection
- All data is stored securely
- Access restricted to authorized personnel
- Personal information protected per privacy laws
- Regular security audits

### Compliance
- HIPAA compliance for medical information
- State and federal privacy laws
- Law enforcement data sharing protocols
- Public information disclosure guidelines

## Maintenance and Updates

### Regular Tasks
- Weekly NamUs data imports
- Monthly data quality reviews
- Quarterly source verification
- Annual system updates

### Monitoring
- Import success rates
- Data accuracy metrics
- Source availability status
- System performance indicators

## Support and Contacts

### Technical Support
- System administrators: admin@241runnersawareness.org
- Database issues: tech@241runnersawareness.org

### Data Source Contacts
- NamUs support: namus@rti.org
- TxDPS missing persons: missingpersons@dps.texas.gov
- HPD missing persons: missingpersons@houstonpolice.org

### Emergency Contacts
- For immediate assistance: 911
- HPD non-emergency: (713) 884-3131
- Harris County Sheriff: (713) 221-6000

---

**Last Updated**: January 2025  
**Version**: 1.0  
**Maintained By**: 241 Runners Awareness Development Team 