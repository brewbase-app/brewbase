CREATE TABLE user_preference_brewing_method
(
    user_preference_id INT NOT NULL,
    brewing_method_id INT NOT NULL,

    PRIMARY KEY
        (
         user_preference_id,
         brewing_method_id
            ),

    CONSTRAINT fk_upbm_preference
        FOREIGN KEY (user_preference_id)
            REFERENCES user_preference(id)
            ON DELETE CASCADE,

    CONSTRAINT fk_upbm_method
        FOREIGN KEY (brewing_method_id)
            REFERENCES brewing_method(id)
            ON DELETE CASCADE
);