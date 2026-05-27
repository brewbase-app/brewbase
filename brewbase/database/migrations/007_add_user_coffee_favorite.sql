CREATE TABLE user_coffee_favorite (
    user_id int NOT NULL,
    coffee_id int NOT NULL,
    created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT user_coffee_favorite_pk PRIMARY KEY (user_id, coffee_id)
);

ALTER TABLE user_coffee_favorite ADD CONSTRAINT user_coffee_favorite_user
    FOREIGN KEY (user_id)
        REFERENCES app_user (id)
        NOT DEFERRABLE
            INITIALLY IMMEDIATE
;

ALTER TABLE user_coffee_favorite ADD CONSTRAINT user_coffee_favorite_coffee
    FOREIGN KEY (coffee_id)
        REFERENCES coffee (id)
        NOT DEFERRABLE
            INITIALLY IMMEDIATE
;

CREATE INDEX idx_user_coffee_favorite_coffee_id ON user_coffee_favorite(coffee_id);
