CREATE PROCEDURE [Framework].[uspAddDistributedLock]
    (
      @pKey VARCHAR(100) ,
      @pDiscriminator VARCHAR(100)
    )
AS
    SET NOCOUNT ON;
    BEGIN
        DECLARE @key VARCHAR(100) = @pKey;
        DECLARE @discriminator VARCHAR(100) = @pDiscriminator;
        INSERT  INTO Framework.DistributedLock
                ( [Key] ,
                  Discriminator ,
                  CreationDateTime
                )
        VALUES  ( @key ,
                  @discriminator ,
                  CURRENT_TIMESTAMP
                );
    END;
