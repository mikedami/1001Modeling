using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace _1001.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "artists",
                columns: table => new
                {
                    artist_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    country = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_artists", x => x.artist_id);
                });

            migrationBuilder.CreateTable(
                name: "songs",
                columns: table => new
                {
                    song_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    release_date = table.Column<DateOnly>(type: "date", nullable: true),
                    duration_seconds = table.Column<int>(type: "integer", nullable: true),
                    genre = table.Column<string>(type: "text", nullable: true),
                    bpm = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_songs", x => x.song_id);
                });

            migrationBuilder.CreateTable(
                name: "venues",
                columns: table => new
                {
                    venue_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    capacity = table.Column<int>(type: "integer", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venues", x => x.venue_id);
                });

            migrationBuilder.CreateTable(
                name: "song_artists",
                columns: table => new
                {
                    song_id = table.Column<int>(type: "integer", nullable: false),
                    artist_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_song_artists", x => new { x.song_id, x.artist_id });
                    table.ForeignKey(
                        name: "FK_song_artists_artists_artist_id",
                        column: x => x.artist_id,
                        principalTable: "artists",
                        principalColumn: "artist_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_song_artists_songs_song_id",
                        column: x => x.song_id,
                        principalTable: "songs",
                        principalColumn: "song_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dj_sets",
                columns: table => new
                {
                    dj_set_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    artist_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: true),
                    set_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    source_url = table.Column<string>(type: "text", nullable: true),
                    venue_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dj_sets", x => x.dj_set_id);
                    table.ForeignKey(
                        name: "FK_dj_sets_artists_artist_id",
                        column: x => x.artist_id,
                        principalTable: "artists",
                        principalColumn: "artist_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dj_sets_venues_venue_id",
                        column: x => x.venue_id,
                        principalTable: "venues",
                        principalColumn: "venue_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "set_analytics",
                columns: table => new
                {
                    dj_set_id = table.Column<int>(type: "integer", nullable: false),
                    tickets_sold = table.Column<int>(type: "integer", nullable: true),
                    attendance_count = table.Column<int>(type: "integer", nullable: true),
                    gross_revenue = table.Column<int>(type: "integer", nullable: true),
                    stream_count = table.Column<int>(type: "integer", nullable: true),
                    like_count = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_set_analytics", x => x.dj_set_id);
                    table.ForeignKey(
                        name: "FK_set_analytics_dj_sets_dj_set_id",
                        column: x => x.dj_set_id,
                        principalTable: "dj_sets",
                        principalColumn: "dj_set_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "set_songs",
                columns: table => new
                {
                    set_song_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    song_id = table.Column<int>(type: "integer", nullable: false),
                    dj_set_id = table.Column<int>(type: "integer", nullable: false),
                    timestamp_in_set_seconds = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_set_songs", x => x.set_song_id);
                    table.ForeignKey(
                        name: "FK_set_songs_dj_sets_dj_set_id",
                        column: x => x.dj_set_id,
                        principalTable: "dj_sets",
                        principalColumn: "dj_set_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_set_songs_songs_song_id",
                        column: x => x.song_id,
                        principalTable: "songs",
                        principalColumn: "song_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dj_sets_artist_id",
                table: "dj_sets",
                column: "artist_id");

            migrationBuilder.CreateIndex(
                name: "IX_dj_sets_venue_id",
                table: "dj_sets",
                column: "venue_id");

            migrationBuilder.CreateIndex(
                name: "IX_set_songs_dj_set_id",
                table: "set_songs",
                column: "dj_set_id");

            migrationBuilder.CreateIndex(
                name: "IX_set_songs_song_id",
                table: "set_songs",
                column: "song_id");

            migrationBuilder.CreateIndex(
                name: "IX_song_artists_artist_id",
                table: "song_artists",
                column: "artist_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "set_analytics");

            migrationBuilder.DropTable(
                name: "set_songs");

            migrationBuilder.DropTable(
                name: "song_artists");

            migrationBuilder.DropTable(
                name: "dj_sets");

            migrationBuilder.DropTable(
                name: "songs");

            migrationBuilder.DropTable(
                name: "artists");

            migrationBuilder.DropTable(
                name: "venues");
        }
    }
}
