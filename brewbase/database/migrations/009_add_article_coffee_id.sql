ALTER TABLE article
    ADD COLUMN coffee_id int NULL;

ALTER TABLE article ADD CONSTRAINT article_coffee_fk
    FOREIGN KEY (coffee_id)
        REFERENCES coffee (id)
        NOT DEFERRABLE
            INITIALLY IMMEDIATE;

CREATE INDEX idx_article_coffee_id ON article(coffee_id);
