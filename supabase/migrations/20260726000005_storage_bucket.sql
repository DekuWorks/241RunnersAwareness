-- Private images bucket (mirrors Azure Blob container "images")
INSERT INTO storage.buckets (id, name, public, file_size_limit, allowed_mime_types)
VALUES (
    'images',
    'images',
    false,
    52428800,
    ARRAY['image/jpeg', 'image/png', 'image/webp', 'image/gif']::text[]
)
ON CONFLICT (id) DO NOTHING;

-- Service role manages uploads; no public anon write
CREATE POLICY images_service_role_all ON storage.objects
    FOR ALL
    TO service_role
    USING (bucket_id = 'images')
    WITH CHECK (bucket_id = 'images');
