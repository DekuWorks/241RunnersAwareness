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

const addMissingColumns = async () => {
    try {
        console.log('🔍 Connecting to Azure SQL Database...');
        await sql.connect(config);
        console.log('✅ Connected to database');
        
        // Check if AdditionalRoles column exists
        console.log('\n📋 Checking for AdditionalRoles column...');
        const checkResult = await sql.query`
            SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AdditionalRoles'
        `;
        
        if (checkResult.recordset.length === 0) {
            console.log('➕ Adding AdditionalRoles column...');
            await sql.query`ALTER TABLE Users ADD AdditionalRoles NVARCHAR(200) NULL`;
            console.log('✅ AdditionalRoles column added successfully');
        } else {
            console.log('✅ AdditionalRoles column already exists');
        }
        
        // Verify the column was added
        console.log('\n🔍 Verifying column details...');
        const verifyResult = await sql.query`
            SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AdditionalRoles'
        `;
        
        if (verifyResult.recordset.length > 0) {
            console.log('Column details:', verifyResult.recordset[0]);
        }
        
        // Update existing admin users
        console.log('\n👥 Updating existing admin users...');
        const updateResult = await sql.query`
            UPDATE Users 
            SET AdditionalRoles = NULL 
            WHERE Role = 'admin' AND AdditionalRoles IS NULL
        `;
        console.log(`Updated ${updateResult.rowsAffected} admin users`);
        
        console.log('\n🎉 Database schema updated successfully!');
        
    } catch (error) {
        console.error('❌ Error:', error.message);
        console.error('Full error:', error);
    } finally {
        await sql.close();
        console.log('\n🔒 Database connection closed.');
    }
};

addMissingColumns().catch(console.error);
