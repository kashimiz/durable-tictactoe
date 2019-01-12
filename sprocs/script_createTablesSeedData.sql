DROP TABLE IF EXISTS gamesession_player;
DROP TABLE IF EXISTS gamesession;
DROP TABLE IF EXISTS player;

CREATE TABLE player (
	ID SERIAL PRIMARY KEY,
    CreatedTime TIMESTAMP NOT NULL,
    UpdatedTime TIMESTAMP NOT NULL,
    Name VARCHAR(100) NOT NULL
);

CREATE TABLE gamesession (
    ID SERIAL PRIMARY KEY,
    GUID VARCHAR(36) NOT NULL,
    CreatedTime TIMESTAMP NOT NULL,
    UpdatedTime TIMESTAMP NOT NULL,
	CreatedPlayer_ID BIGINT UNSIGNED NOT NULL,
    GameStatus TINYINT NOT NULL,
    BoardState VARCHAR(200) NOT NULL,
    MovesLeft TINYINT NOT NULL,
    CurrentTurnPlayer_ID BIGINT UNSIGNED,
    WinningPlayer_ID BIGINT UNSIGNED,
    FOREIGN KEY(CreatedPlayer_ID) REFERENCES player(ID),
    FOREIGN KEY(CurrentTurnPlayer_ID) REFERENCES player(ID),
    FOREIGN KEY(WinningPlayer_ID) REFERENCES player(ID)
);

CREATE TABLE gamesession_player (
	ID SERIAL PRIMARY KEY,
    CreatedTime TIMESTAMP NOT NULL,
    GameSession_ID BIGINT UNSIGNED NOT NULL,
    Player_ID BIGINT UNSIGNED NOT NULL,
    FOREIGN KEY(GameSession_ID) REFERENCES gamesession(ID),
    FOREIGN KEY(Player_ID) REFERENCES player(ID)
);

INSERT INTO player 
  (CreatedTime, UpdatedTime, Name)
VALUES 
  (now(), now(), "Simon"),
  (now(), now(), "Pepper"),
  (now(), now(), "Boris");