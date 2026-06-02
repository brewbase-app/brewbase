CREATE TABLE user_preference_region
(
    user_preference_id INT NOT NULL,
    region_id INT NOT NULL,

    PRIMARY KEY
        (
         user_preference_id,
         region_id
            ),

    CONSTRAINT fk_upr_preference
        FOREIGN KEY (user_preference_id)
            REFERENCES user_preference(id)
            ON DELETE CASCADE,

    CONSTRAINT fk_upr_region
        FOREIGN KEY (region_id)
            REFERENCES region(id)
            ON DELETE CASCADE
);
