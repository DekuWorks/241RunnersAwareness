.PHONY: supabase-init supabase-push migrate-dry migrate docs

supabase-init:
	supabase init

supabase-push:
	bash scripts/supabase-push-migrations.sh

migrate-dry:
	MIGRATION_DRY_RUN=true bash scripts/run-data-migration.sh

migrate:
	MIGRATION_DRY_RUN=false bash scripts/run-data-migration.sh

docs:
	@echo "Migration docs: docs/migration/README.md"
