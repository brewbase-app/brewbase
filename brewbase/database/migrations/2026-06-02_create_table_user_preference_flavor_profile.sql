CREATE TABLE user_preference_flavor_profile
(
    user_preference_id INT NOT NULL,
    flavor_profile_id INT NOT NULL,

    PRIMARY KEY
        (
         user_preference_id,
         flavor_profile_id
            ),

    CONSTRAINT fk_user_preference_flavor_profile_preference
        FOREIGN KEY (user_preference_id)
            REFERENCES user_preference(id)
            ON DELETE CASCADE,

    CONSTRAINT fk_user_preference_flavor_profile_flavor
        FOREIGN KEY (flavor_profile_id)
            REFERENCES flavor_profile(id)
            ON DELETE CASCADE
);