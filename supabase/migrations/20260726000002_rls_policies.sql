-- Row Level Security: deny direct client access to application tables.
-- The ASP.NET API connects with a privileged DB role (postgres / service) and bypasses RLS.
-- Public read access is exposed only through v_public_* views (next migration).

ALTER TABLE "Users" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "Runners" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "Cases" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "Devices" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "TopicSubscriptions" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "Notifications" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "DataDeletionRequests" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "AccountDeletionRequests" ENABLE ROW LEVEL SECURITY;

CREATE POLICY users_deny_all ON "Users" FOR ALL USING (FALSE);
CREATE POLICY runners_deny_all ON "Runners" FOR ALL USING (FALSE);
CREATE POLICY cases_deny_all ON "Cases" FOR ALL USING (FALSE);
CREATE POLICY devices_deny_all ON "Devices" FOR ALL USING (FALSE);
CREATE POLICY topic_subscriptions_deny_all ON "TopicSubscriptions" FOR ALL USING (FALSE);
CREATE POLICY notifications_deny_all ON "Notifications" FOR ALL USING (FALSE);
CREATE POLICY data_deletion_requests_deny_all ON "DataDeletionRequests" FOR ALL USING (FALSE);
CREATE POLICY account_deletion_requests_deny_all ON "AccountDeletionRequests" FOR ALL USING (FALSE);

-- Revoke direct table access from anon/authenticated Supabase roles
REVOKE ALL ON TABLE "Users" FROM anon, authenticated;
REVOKE ALL ON TABLE "Runners" FROM anon, authenticated;
REVOKE ALL ON TABLE "Cases" FROM anon, authenticated;
REVOKE ALL ON TABLE "Devices" FROM anon, authenticated;
REVOKE ALL ON TABLE "TopicSubscriptions" FROM anon, authenticated;
REVOKE ALL ON TABLE "Notifications" FROM anon, authenticated;
REVOKE ALL ON TABLE "DataDeletionRequests" FROM anon, authenticated;
REVOKE ALL ON TABLE "AccountDeletionRequests" FROM anon, authenticated;
