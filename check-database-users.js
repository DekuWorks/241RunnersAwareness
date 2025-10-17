const sql = require('mssql');

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

const checkDatabaseUsers = async () => {
    try {
        console.log('🔍 Connecting to Azure SQL Database...');
        await sql.connect(config);
        console.log('✅ Connected to database successfully!');
        
        // Check if Users table exists and get its structure
        console.log('\n📋 Checking Users table structure...');
        const tableInfo = await sql.query`
            SELECT 
                COLUMN_NAME, 
                DATA_TYPE, 
                IS_NULLABLE,
                COLUMN_DEFAULT
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'Users'
            ORDER BY ORDINAL_POSITION
        `;
        
        console.log('Users table columns:');
        tableInfo.recordset.forEach(col => {
            console.log(`  - ${col.COLUMN_NAME} (${col.DATA_TYPE}) ${col.IS_NULLABLE === 'YES' ? 'NULL' : 'NOT NULL'}`);
        });
        
        // Check user count
        console.log('\n👥 Checking user count...');
        const userCount = await sql.query`SELECT COUNT(*) as UserCount FROM Users`;
        console.log(`Total users in database: ${userCount.recordset[0].UserCount}`);
        
        // Check admin users specifically
        console.log('\n🔐 Checking admin users...');
        const adminUsers = await sql.query`
            SELECT 
                Id, 
                Email, 
                FirstName, 
                LastName, 
                Role, 
                IsActive,
                CreatedAt
            FROM Users
            WHERE Role = 'admin'
            ORDER BY FirstName, LastName
        `;
        
        console.log(`Found ${adminUsers.recordset.length} admin users:`);
        adminUsers.recordset.forEach(user => {
            console.log(`  - ${user.FirstName} ${user.LastName} (${user.Email}) - Active: ${user.IsActive}`);
        });
        
        // Check if any users have the specific emails we created
        console.log('\n📧 Checking specific admin emails...');
        const specificUsers = await sql.query`
            SELECT Email, FirstName, LastName, Role, IsActive
            FROM Users 
            WHERE Email IN (
                'dekuworks1@gmail.com',
                'lthomas3350@gmail.com',
                'markmelasky@gmail.com',
                'danielcarey9770@yahoo.com',
                'tinaleggins@yahoo.com',
                'ralphfrank900@gmail.com'
            )
        `;
        
        console.log(`Found ${specificUsers.recordset.length} of our 6 admin users:`);
        specificUsers.recordset.forEach(user => {
            console.log(`  - ${user.Email} (${user.FirstName} ${user.LastName}) - Role: ${user.Role} - Active: ${user.IsActive}`);
        });
        
    } catch (error) {
        console.error('❌ Database error:', error.message);
        console.error('Full error:', error);
    } finally {
        await sql.close();
        console.log('\n🔒 Database connection closed.');
    }
};

checkDatabaseUsers().catch(console.error);
