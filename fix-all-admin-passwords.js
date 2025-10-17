const sql = require('mssql');
const bcrypt = require('bcrypt');

const config = {
    server: '241runners-sql-2025.database.windows.net',
    database: '241RunnersAwarenessDB',
    user: 'sqladmin',
    password: 'AdminPass123!',
    options: {
        encrypt: true,
        trustServerCertificate: false
    }
};

const adminUsers = [
    { email: 'dekuworks1@gmail.com', password: 'marcus2025', name: 'Marcus Brown' },
    { email: 'lthomas3350@gmail.com', password: 'Lisa2025!', name: 'Lisa Thomas' },
    { email: 'markmelasky@gmail.com', password: 'Mark2025!', name: 'Mark Melasky' },
    { email: 'danielcarey9770@yahoo.com', password: 'Daniel2025!', name: 'Daniel Carey' },
    { email: 'tinaleggins@yahoo.com', password: 'Tina2025!', name: 'Tina Matthews' },
    { email: 'ralphfrank900@gmail.com', password: 'Ralph2025!', name: 'Ralph Frank' }
];

const fixAllAdminPasswords = async () => {
    try {
        console.log('🔧 Fixing all admin user passwords...\n');
        await sql.connect(config);
        
        for (const user of adminUsers) {
            console.log(`🔐 Processing ${user.name} (${user.email})...`);
            
            // Generate correct password hash
            const saltRounds = 11;
            const correctHash = await bcrypt.hash(user.password, saltRounds);
            
            // Update the password hash in the database
            await sql.query`
                UPDATE Users 
                SET PasswordHash = ${correctHash}
                WHERE Email = ${user.email}
            `;
            
            console.log(`✅ Updated password for ${user.name}`);
        }
        
        console.log('\n🎉 All admin passwords have been fixed!');
        
        // Test one of the logins
        console.log('\n🧪 Testing Marcus login...');
        const testResult = await sql.query`
            SELECT Email, PasswordHash, FirstName, LastName
            FROM Users 
            WHERE Email = 'dekuworks1@gmail.com'
        `;
        
        if (testResult.recordset.length > 0) {
            const user = testResult.recordset[0];
            const isValid = await bcrypt.compare('marcus2025', user.PasswordHash);
            console.log(`✅ Marcus password verification: ${isValid ? 'SUCCESS' : 'FAILED'}`);
        }
        
    } catch (error) {
        console.error('❌ Error:', error.message);
    } finally {
        await sql.close();
    }
};

fixAllAdminPasswords().catch(console.error);
