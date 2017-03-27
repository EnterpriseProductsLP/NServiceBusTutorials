CREATE PROCEDURE [Framework].[uspHeartbeatDistributedLock]
    (
      @success BIT OUTPUT ,				-- Output.  1 if call succesfful aquires or updates the lock.  Otherwise 0.
      @pKey VARCHAR(100) ,				-- The unique key for the requested lock.
      @pDiscriminator VARCHAR(100) ,	-- The caller's unique discriminator.
      @pHeartbeatDuration INT			-- The number of seconds where a heartbeat is considered valid by the caller.
    )
AS
    SET NOCOUNT ON;
    BEGIN
        DECLARE @key VARCHAR(100) = @pKey;
        DECLARE @discriminator VARCHAR(100) = @pDiscriminator;
        DECLARE @heartbeatDuration INT = @pHeartbeatDuration;
        BEGIN TRANSACTION Heartbeat;


		-- Get a count of matching locks by KEY ONLY
        DECLARE @lockHeartbeat DATETIME;
        SELECT  @lockHeartbeat = Heartbeat
        FROM    Framework.DistributedLock
        WHERE   [Key] = @key;

		-- Get a count of existing locks that belong to the caller
        DECLARE @discriminatedHeartbeat DATETIME;
        SELECT  @discriminatedHeartbeat = Heartbeat
        FROM    Framework.DistributedLock
        WHERE   [Key] = @key
                AND Discriminator = @discriminator;

		-- Get a count of locks that are still valid based on the heartbeat duration.
        DECLARE @validHeartbeat DATETIME;
        SELECT  @validHeartbeat = Heartbeat
        FROM    Framework.DistributedLock
        WHERE   [Key] = @key
                AND DATEADD(SECOND, @heartbeatDuration, Heartbeat) > CURRENT_TIMESTAMP;

        IF ( @lockHeartbeat IS NULL )
			-- If there is no lock yet, insert one.
            BEGIN
                INSERT  INTO Framework.DistributedLock
                        ( [Key] ,
                          Discriminator ,
                          Heartbeat
                        )
                VALUES  ( @key ,
                          @discriminator ,
                          CURRENT_TIMESTAMP
                        );
                SET @success = 1;
            END;

        ELSE
            IF ( @discriminatedHeartbeat IS NOT NULL )
				-- If there is a lock and it belongs to the caller, update the lock.
                BEGIN
                    UPDATE  Framework.DistributedLock
                    SET     Heartbeat = CURRENT_TIMESTAMP
                    WHERE   [Key] = @key
                            AND Discriminator = @discriminator
                            AND Heartbeat = @discriminatedHeartbeat;
                    SET @success = @@ROWCOUNT;
                END;
            

            ELSE
                IF ( @validHeartbeat IS NULL )
					-- If there is a lock, but it has expired
					-- Take the lock
                    BEGIN
                        UPDATE  Framework.DistributedLock
                        SET     Heartbeat = CURRENT_TIMESTAMP ,
                                Discriminator = @discriminator
                        WHERE   [Key] = @key
                                AND Heartbeat = @lockHeartbeat;
                        SET @success = @@ROWCOUNT;
                    END;
                ELSE
                    BEGIN 
                        SET @success = 0;
                    END;
        COMMIT TRANSACTION Heartbeat;
    END;
