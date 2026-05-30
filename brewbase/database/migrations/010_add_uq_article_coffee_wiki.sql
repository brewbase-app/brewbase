CREATE UNIQUE INDEX uq_article_coffee_wiki
    ON article(coffee_id)
    WHERE module = 'coffee'
      AND status = 'Approved'
      AND coffee_id IS NOT NULL;
