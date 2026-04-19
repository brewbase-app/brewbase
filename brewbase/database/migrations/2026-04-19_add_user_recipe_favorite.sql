CREATE TABLE user_recipe_favorite (
                                      user_id int NOT NULL,
                                      recipe_id int NOT NULL,
                                      created_at timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                      CONSTRAINT user_recipe_favorite_pk PRIMARY KEY (user_id, recipe_id)
);

ALTER TABLE user_recipe_favorite ADD CONSTRAINT user_recipe_favorite_user
    FOREIGN KEY (user_id)
        REFERENCES app_user (id)
        NOT DEFERRABLE
            INITIALLY IMMEDIATE
;

ALTER TABLE user_recipe_favorite ADD CONSTRAINT user_recipe_favorite_recipe
    FOREIGN KEY (recipe_id)
        REFERENCES recipe (id)
        NOT DEFERRABLE
            INITIALLY IMMEDIATE
;

CREATE INDEX idx_user_recipe_favorite_recipe_id ON user_recipe_favorite(recipe_id);