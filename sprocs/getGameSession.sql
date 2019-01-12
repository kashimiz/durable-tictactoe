DELIMITER //
DROP PROCEDURE IF EXISTS `getGameSession`;

CREATE PROCEDURE `getGameSession`(IN param_guid VARCHAR(36))
BEGIN
	SELECT * FROM gamesession
    WHERE id != 0 AND guid = param_guid
    LIMIT 1;
    
    SELECT Player_ID FROM gamesession_player AS gsp
    LEFT JOIN gamesession AS g ON (g.ID = gsp.GameSession_ID)
    WHERE g.ID != 0 AND g.GUID = param_guid;
END//
DELIMITER ;
