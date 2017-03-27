CREATE PROCEDURE [Framework].[uspRemoveDistributedLock]
    (
      @pKey VARCHAR(100) ,			-- The unique key for the requested lock.
      @pDiscriminator VARCHAR(100)	-- The caller's unique discriminator.
    )
AS
    SET NOCOUNT ON;
    BEGIN
        DECLARE @key VARCHAR(100) = @pKey;
        DECLARE @discriminator VARCHAR(100) = @pDiscriminator;

        DELETE  FROM Framework.DistributedLock
        WHERE   [Key] = @key
                AND Discriminator = @discriminator;
    END;
