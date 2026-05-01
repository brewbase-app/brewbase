using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Models;

public partial class BrewDbContext : DbContext
{
    public BrewDbContext()
    {
    }

    public BrewDbContext(DbContextOptions<BrewDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Article> Articles { get; set; }

    public virtual DbSet<BrewingMethod> BrewingMethods { get; set; }

    public virtual DbSet<Coffee> Coffees { get; set; }

    public virtual DbSet<CoffeeRanking> CoffeeRankings { get; set; }

    public virtual DbSet<CoffeeRating> CoffeeRatings { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<CuppingSession> CuppingSessions { get; set; }
    
    public virtual DbSet<CuppingSessionCoffee> CuppingSessionCoffees { get; set; }

    public virtual DbSet<Follow> Follows { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<ProcessingMethod> ProcessingMethods { get; set; }

    public virtual DbSet<QuickNote> QuickNotes { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RecipeRanking> RecipeRankings { get; set; }

    public virtual DbSet<RecipeRating> RecipeRatings { get; set; }

    public virtual DbSet<Recommendation> Recommendations { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<Roastery> Roasteries { get; set; }

    public virtual DbSet<UserPreference> UserPreferences { get; set; }

    public virtual DbSet<UserRanking> UserRankings { get; set; }

    public virtual DbSet<UserRecipeFavorite> UserRecipeFavorites { get; set; }

    public virtual DbSet<Variety> Varieties { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("app_user_pk");

            entity.ToTable("app_user");

            entity.HasIndex(e => e.Email, "app_user_email_key").IsUnique();

            entity.HasIndex(e => e.Login, "app_user_login_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityPoints)
                .HasDefaultValue(0)
                .HasColumnName("activity_points");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsBlocked)
                .HasDefaultValue(false)
                .HasColumnName("is_blocked");
            entity.Property(e => e.Label)
                .HasMaxLength(255)
                .HasColumnName("label");
            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .HasColumnName("login");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
        });

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("article_pk");

            entity.ToTable("article");

            entity.HasIndex(e => e.ModeratedByUserId, "idx_article_moderated_by_user_id");

            entity.HasIndex(e => e.UserId, "idx_article_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ModeratedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("moderated_at");
            entity.Property(e => e.ModeratedByUserId).HasColumnName("moderated_by_user_id");
            entity.Property(e => e.ModerationComment).HasColumnName("moderation_comment");
            entity.Property(e => e.PublishedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("published_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ModeratedByUser).WithMany(p => p.ArticleModeratedByUsers)
                .HasForeignKey(d => d.ModeratedByUserId)
                .HasConstraintName("article_moderator_user");

            entity.HasOne(d => d.User).WithMany(p => p.ArticleUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("article_user");
        });

        modelBuilder.Entity<BrewingMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("brewing_method_pk");

            entity.ToTable("brewing_method");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Coffee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("coffee_pk");

            entity.ToTable("coffee");

            entity.HasIndex(e => e.CreatedByUserId, "idx_coffee_created_by_user_id");

            entity.HasIndex(e => e.ProcessingMethodId, "idx_coffee_processing_method_id");

            entity.HasIndex(e => e.RegionId, "idx_coffee_region_id");

            entity.HasIndex(e => e.RoasteryId, "idx_coffee_roastery_id");

            entity.HasIndex(e => e.VarietyId, "idx_coffee_variety_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ProcessingMethodId).HasColumnName("processing_method_id");
            entity.Property(e => e.RegionId).HasColumnName("region_id");
            entity.Property(e => e.RoasteryId).HasColumnName("roastery_id");
            entity.Property(e => e.VarietyId).HasColumnName("variety_id");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Coffees)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("coffee_user");

            entity.HasOne(d => d.ProcessingMethod).WithMany(p => p.Coffees)
                .HasForeignKey(d => d.ProcessingMethodId)
                .HasConstraintName("coffee_processing_methods");

            entity.HasOne(d => d.Region).WithMany(p => p.Coffees)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("coffee_regions");

            entity.HasOne(d => d.Roastery).WithMany(p => p.Coffees)
                .HasForeignKey(d => d.RoasteryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("coffee_roasteries");

            entity.HasOne(d => d.Variety).WithMany(p => p.Coffees)
                .HasForeignKey(d => d.VarietyId)
                .HasConstraintName("coffee_varieties");
        });

        modelBuilder.Entity<CoffeeRanking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("coffee_ranking_pk");

            entity.ToTable("coffee_ranking");

            entity.HasIndex(e => e.CoffeeId, "idx_coffee_ranking_coffee_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CoffeeId).HasColumnName("coffee_id");
            entity.Property(e => e.LikeCount).HasColumnName("like_count");
            entity.Property(e => e.RatingCount).HasColumnName("rating_count");
            entity.Property(e => e.RecipeUsedCount).HasColumnName("recipe_used_count");
            entity.Property(e => e.RefreshedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("refreshed_at");

            entity.HasOne(d => d.Coffee).WithMany(p => p.CoffeeRankings)
                .HasForeignKey(d => d.CoffeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ranking_coffee_coffee");
        });

        modelBuilder.Entity<CoffeeRating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("coffee_rating_pk");

            entity.ToTable("coffee_rating");

            entity.HasIndex(e => e.CoffeeId, "idx_coffee_rating_coffee_id");

            entity.HasIndex(e => e.UserId, "idx_coffee_rating_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CoffeeId).HasColumnName("coffee_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Coffee).WithMany(p => p.CoffeeRatings)
                .HasForeignKey(d => d.CoffeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rating_coffee_coffee");

            entity.HasOne(d => d.User).WithMany(p => p.CoffeeRatings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rating_coffee_user");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("country_pk");

            entity.ToTable("country");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CuppingSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cupping_session_pk");

            entity.ToTable("cupping_session");

            entity.HasIndex(e => e.UserId, "idx_cupping_session_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.CuppingSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cupping_session_user");
        });
        
        modelBuilder.Entity<CuppingSessionCoffee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cupping_session_coffee_pk");

            entity.ToTable("cupping_session_coffee");

            entity.HasIndex(e => e.CoffeeId, "idx_cupping_session_coffee_coffee_id");

            entity.HasIndex(e => e.CuppingSessionId, "idx_cupping_session_coffee_cupping_session_id");

            entity.HasIndex(e => new { e.CuppingSessionId, e.CoffeeId }, "uq_session_coffee").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CoffeeId).HasColumnName("coffee_id");
            entity.Property(e => e.CuppingSessionId).HasColumnName("cupping_session_id");

            entity.HasOne(d => d.Coffee).WithMany(p => p.CuppingSessionCoffees)
                .HasForeignKey(d => d.CoffeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cupping_session_coffee_coffee");

            entity.HasOne(d => d.CuppingSession).WithMany(p => p.CuppingSessionCoffees)
                .HasForeignKey(d => d.CuppingSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cupping_session_coffee_session");
        });

        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasKey(e => new { e.FollowerId, e.FollowedId }).HasName("follow_pk");

            entity.ToTable("follow");

            entity.HasIndex(e => e.FollowedId, "idx_follow_followed_id");

            entity.Property(e => e.FollowerId).HasColumnName("follower_id");
            entity.Property(e => e.FollowedId).HasColumnName("followed_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Followed).WithMany(p => p.FollowFolloweds)
                .HasForeignKey(d => d.FollowedId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("followed_user");

            entity.HasOne(d => d.Follower).WithMany(p => p.FollowFollowers)
                .HasForeignKey(d => d.FollowerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("follower_user");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_pk");

            entity.ToTable("notification");

            entity.HasIndex(e => e.UserId, "idx_notification_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("notification_user");
        });

        modelBuilder.Entity<ProcessingMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("processing_method_pk");

            entity.ToTable("processing_method");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<QuickNote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quick_note_pk");

            entity.ToTable("quick_note");

            entity.HasIndex(e => e.UserId, "idx_quick_note_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.QuickNotes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("quick_notes_user");
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("recipe_pk");

            entity.ToTable("recipe");

            entity.HasIndex(e => e.BrewingMethodId, "idx_recipe_brewing_method_id");

            entity.HasIndex(e => e.CoffeeId, "idx_recipe_coffee_id");

            entity.HasIndex(e => e.UserId, "idx_recipe_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BrewingMethodId).HasColumnName("brewing_method_id");
            entity.Property(e => e.CoffeeId).HasColumnName("coffee_id");
            entity.Property(e => e.IsPublic).HasColumnName("is_public");
            entity.Property(e => e.Parameters)
                .HasColumnType("jsonb")
                .HasColumnName("parameters");
            entity.Property(e => e.Steps).HasColumnName("steps");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.BrewingMethod).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.BrewingMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recipes_brewing_methods");

            entity.HasOne(d => d.Coffee).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.CoffeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recipes_coffee");

            entity.HasOne(d => d.User).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recipes_user");
        });

        modelBuilder.Entity<RecipeRanking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("recipe_ranking_pk");

            entity.ToTable("recipe_ranking");

            entity.HasIndex(e => e.RecipeId, "idx_recipe_ranking_recipe_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LikeCount).HasColumnName("like_count");
            entity.Property(e => e.RatingCount).HasColumnName("rating_count");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.RefreshedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("refreshed_at");
            entity.Property(e => e.SaveCount).HasColumnName("save_count");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeRankings)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ranking_recipe_recipe");
        });

        modelBuilder.Entity<RecipeRating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("recipe_rating_pk");

            entity.ToTable("recipe_rating");

            entity.HasIndex(e => e.RecipeId, "idx_recipe_rating_recipe_id");

            entity.HasIndex(e => e.UserId, "idx_recipe_rating_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeRatings)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rating_recipe_recipes");

            entity.HasOne(d => d.User).WithMany(p => p.RecipeRatings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rating_recipe_user");
        });

        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("recommendations_pk");

            entity.ToTable("recommendations");

            entity.HasIndex(e => e.CoffeeId, "idx_recommendations_coffee_id");

            entity.HasIndex(e => e.CoffeeRankingId, "idx_recommendations_coffee_ranking_id");

            entity.HasIndex(e => e.RecipeId, "idx_recommendations_recipe_id");

            entity.HasIndex(e => e.UserId, "idx_recommendations_user_id");

            entity.HasIndex(e => e.UserPreferenceId, "idx_recommendations_user_preference_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Algorithm)
                .HasMaxLength(30)
                .HasColumnName("algorithm");
            entity.Property(e => e.CoffeeId).HasColumnName("coffee_id");
            entity.Property(e => e.CoffeeRankingId).HasColumnName("coffee_ranking_id");
            entity.Property(e => e.Feedback).HasColumnName("feedback");
            entity.Property(e => e.FeedbackAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("feedback_at");
            entity.Property(e => e.GeneratedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("generated_at");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Source).HasColumnName("source");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserPreferenceId).HasColumnName("user_preference_id");

            entity.HasOne(d => d.Coffee).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.CoffeeId)
                .HasConstraintName("recommendations_coffee");

            entity.HasOne(d => d.CoffeeRanking).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.CoffeeRankingId)
                .HasConstraintName("recommendations_ranking_coffee");

            entity.HasOne(d => d.Recipe).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("recommendations_recipe");

            entity.HasOne(d => d.User).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("recommendations_user");

            entity.HasOne(d => d.UserPreference).WithMany(p => p.Recommendations)
                .HasForeignKey(d => d.UserPreferenceId)
                .HasConstraintName("recommendations_user_preferences");
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("region_pk");

            entity.ToTable("region");

            entity.HasIndex(e => e.CountryId, "idx_region_country_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.Country).WithMany(p => p.Regions)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("region_country");
        });

        modelBuilder.Entity<Roastery>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roastery_pk");

            entity.ToTable("roastery");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_preference_pk");

            entity.ToTable("user_preference");

            entity.HasIndex(e => e.UserId, "idx_user_preference_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FavoriteNotes).HasColumnName("favorite_notes");
            entity.Property(e => e.PreferredRoastLevel)
                .HasMaxLength(255)
                .HasColumnName("preferred_roast_level");
            entity.Property(e => e.QuizCompleted).HasColumnName("quiz_completed");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserPreferences)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_preferences_user");
        });

        modelBuilder.Entity<UserRanking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_ranking_pk");

            entity.ToTable("user_ranking");

            entity.HasIndex(e => e.UserId, "idx_user_ranking_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityScore).HasColumnName("activity_score");
            entity.Property(e => e.LikeCount).HasColumnName("like_count");
            entity.Property(e => e.RecipeCount).HasColumnName("recipe_count");
            entity.Property(e => e.RefreshedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("refreshed_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserRankings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ranking_user_user");
        });

        modelBuilder.Entity<UserRecipeFavorite>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RecipeId }).HasName("user_recipe_favorite_pk");

            entity.ToTable("user_recipe_favorite");

            entity.HasIndex(e => e.RecipeId, "idx_user_recipe_favorite_recipe_id");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Recipe).WithMany(p => p.UserRecipeFavorites)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_recipe_favorite_recipe");

            entity.HasOne(d => d.User).WithMany(p => p.UserRecipeFavorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_recipe_favorite_user");
        });

        modelBuilder.Entity<Variety>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("variety_pk");

            entity.ToTable("variety");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
