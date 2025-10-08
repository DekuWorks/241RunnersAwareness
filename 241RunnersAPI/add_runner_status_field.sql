-- Add Status field to Runner table
-- This script adds the Status field to the Runner table as required by the specification

-- Add the Status column
ALTER TABLE [Runners] ADD [Status] nvarchar(50) NOT NULL DEFAULT 'Missing';

-- Add index for performance
CREATE INDEX [IX_Runners_Status] ON [Runners] ([Status]);

-- Update existing runners to have 'Missing' status
UPDATE [Runners] SET [Status] = 'Missing' WHERE [Status] IS NULL;

-- Add check constraint to ensure valid status values
ALTER TABLE [Runners] ADD CONSTRAINT [CK_Runners_Status] CHECK ([Status] IN ('Missing', 'Found', 'Resolved'));
