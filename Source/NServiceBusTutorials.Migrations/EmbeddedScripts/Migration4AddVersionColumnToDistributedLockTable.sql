ALTER TABLE Framework.DistributedLock ADD
Version BIGINT NOT NULL CONSTRAINT DF_DistributedLock_Version DEFAULT 1;
GO

ALTER TABLE Framework.DistributedLock SET (LOCK_ESCALATION = TABLE);
GO
