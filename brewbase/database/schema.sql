CREATE TABLE Users (
                       Id SERIAL PRIMARY KEY,

                       Email VARCHAR(255) NOT NULL,
                       PasswordHash VARCHAR(255) NOT NULL,
                       Login VARCHAR(100) NOT NULL,
                       Role VARCHAR(50) NOT NULL,

                       ActivityPoints INT NOT NULL DEFAULT 0,
                       Label VARCHAR(100),

                       IsBlocked BOOLEAN NOT NULL DEFAULT FALSE,

                       CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);