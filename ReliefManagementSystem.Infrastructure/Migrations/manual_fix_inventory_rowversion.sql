ALTER TABLE "InventoryStocks"
ALTER COLUMN "RowVersion" SET DEFAULT '\x00'::bytea;

UPDATE "InventoryStocks"
SET "RowVersion" = '\x00'::bytea
WHERE "RowVersion" IS NULL;
