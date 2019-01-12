DELIMITER //
DROP PROCEDURE IF EXISTS `getGameSessionList`;

CREATE PROCEDURE `getGameSessionList`(IN param_player_id BIGINT UNSIGNED)
BEGIN
	SELECT * FROM gamesession_player AS gsp
    LEFT JOIN gamesession AS g ON (gsp.GameSession_ID = g.ID)
    WHERE gsp.ID != 0 AND gsp.Player_ID = param_player_id;
END//
DELIMITER ;
