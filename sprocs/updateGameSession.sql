DELIMITER //
DROP PROCEDURE IF EXISTS `updateGameSession`;

CREATE PROCEDURE `updateGameSession`
   (IN param_guid VARCHAR(36),
	IN param_status TINYINT,
    IN param_boardstate VARCHAR(200),
    IN param_movesleft TINYINT,
    IN param_currentturnplayer_id BIGINT UNSIGNED,
    IN param_winningplayer_id BIGINT UNSIGNED)
BEGIN
	DECLARE exit handler for SQLEXCEPTION
    BEGIN
		GET DIAGNOSTICS CONDITION 1 @sqlstate = RETURNED_SQLSTATE,
			@errno = MYSQL_ERRNO, @text = MESSAGE_TEXT;
		SET @full_error = CONCAT("ERROR ", @errno, " (", @sqlstate, "): ",
			@text);
		SELECT @full_error;
    END;

	UPDATE `gamesession`
    SET
		`UpdatedTime` = NOW(),
		`GameStatus` = param_status,
        `BoardState` = param_boardstate,
        `MovesLeft` = param_movesleft,
        `CurrentTurnPlayer_ID` = param_currentturnplayer_id,
        `WinningPlayer_ID` = param_winningplayer_id
	WHERE `gamesession`.ID != 0 AND `gamesession`.GUID = param_guid;
    
    SELECT CONCAT("Success: ", ROW_COUNT());
END
//