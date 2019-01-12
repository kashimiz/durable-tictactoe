DELIMITER //
DROP PROCEDURE IF EXISTS `createOrJoinGameSession`;

CREATE PROCEDURE `createOrJoinGameSession`
	(IN param_player_id BIGINT UNSIGNED)
BEGIN
	DECLARE sessionId BIGINT DEFAULT NULL;
    DECLARE sessionGuid VARCHAR(36);

	# locate existing game matching request
	SELECT ID, GUID INTO sessionId, sessionGuid FROM gamesession
    WHERE GameStatus = 0 AND CreatedPlayer_ID != param_player_id
	ORDER BY CreatedTime
    LIMIT 1;
    
    IF sessionId IS NULL THEN
		SET sessionGuid = UUID();
        
        INSERT INTO gamesession
			(GUID,
			CreatedTime,
			UpdatedTime,
			CreatedPlayer_ID,
			GameStatus,
			BoardState,
			MovesLeft)
		VALUES 
			(sessionGuid,
			 NOW(),
			 NOW(),
			 param_player_id,
			 0,
			 "[[0,0,0],[0,0,0],[0,0,0]]",
			 9);
             
		SET sessionId = LAST_INSERT_ID();
	ELSE
		UPDATE gamesession
        SET GameStatus = 1
        WHERE ID = sessionId;
    END IF;
    
    INSERT INTO gamesession_player
			(CreatedTime,
			GameSession_ID,
			Player_ID)
		VALUES
			(NOW(),
			sessionId,
			param_player_id);
    
    SELECT sessionId AS id, sessionGuid AS guid;
END
//
