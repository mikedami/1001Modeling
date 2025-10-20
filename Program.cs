using _1001;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;


public static class Program
{
    static void Main(string[] args)
    {
        using var context = new AppDbContext();

        // Ensure database is created
        AnsiConsole.Status()
            .Start("Initializing database...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("green"));
                context.Database.EnsureCreated();
            });

        ShowWelcomeBanner();

        while (true)
        {
            AnsiConsole.Clear();
            ShowWelcomeBanner();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold cyan]Main Menu[/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "Add new DJ set",
                        "Query database",
                        "Update DJ set",
                        "Delete DJ set",
                        "Exit"
                    }));

            AnsiConsole.Clear();

            switch (choice)
            {
                case "Add new DJ set":
                    AddDjSet(context);
                    AnsiConsole.MarkupLine("\n[green]DJ Set added successfully![/]");
                    AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    break;
                case "Query database":
                    QueryDatabase(context);
                    break;
                case "Update DJ set":
                    UpdateDjSet(context);
                    break;
                case "Delete DJ set":
                    DeleteDjSet(context);
                    break;
                case "Exit":
                    ShowGoodbyeMessage();
                    return;
            }
        }
    }

    static void ShowWelcomeBanner()
    {
        var banner = new FigletText("DJ SETS")
            .Centered()
            .Color(Color.Cyan1);

        AnsiConsole.Write(banner);

        var rule = new Rule("[yellow]Management System[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    static void ShowGoodbyeMessage()
    {
        AnsiConsole.Clear();
        var panel = new Panel(
            new Markup("[cyan]Thanks for using[/] [bold yellow]DJ Set Management System[/]!\n[dim]See you next time![/]")
                .Centered())
        {
            Border = BoxBorder.Double,
            BorderStyle = new Style(Color.Cyan1)
        };
        AnsiConsole.Write(panel);
    }

    static void QueryDatabase(AppDbContext context)
    {
        while (true)
        {
            AnsiConsole.Clear();

            var rule = new Rule("[cyan]Query Database[/]")
            {
                Justification = Justify.Left
            };
            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Select a query option:[/]")
                    .PageSize(15)
                    .AddChoices(new[] {
                        "Search DJ sets by artist",
                        "Search DJ sets by venue",
                        "Search DJ sets by date range",
                        "View all DJ sets",
                        "Search songs by title",
                        "Search songs by artist",
                        "Search songs by genre",
                        "View DJ set details (with songs)",
                        "View set analytics",
                        "Back to main menu"
                    }));

            AnsiConsole.Clear();

            switch (choice)
            {
                case "Search DJ sets by artist":
                    SearchDjSetsByArtist(context);
                    break;
                case "Search DJ sets by venue":
                    SearchDjSetsByVenue(context);
                    break;
                case "Search DJ sets by date range":
                    SearchDjSetsByDateRange(context);
                    break;
                case "View all DJ sets":
                    ViewAllDjSets(context);
                    break;
                case "Search songs by title":
                    SearchSongsByTitle(context);
                    break;
                case "Search songs by artist":
                    SearchSongsByArtist(context);
                    break;
                case "Search songs by genre":
                    SearchSongsByGenre(context);
                    break;
                case "View DJ set details (with songs)":
                    ViewDjSetDetails(context);
                    break;
                case "View set analytics":
                    ViewSetAnalytics(context);
                    break;
                case "Back to main menu":
                    return;
            }

            if (choice != "Back to main menu")
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
                Console.ReadKey(true);
            }
        }
    }

    static void SearchDjSetsByArtist(AppDbContext context)
    {
        var searchName = AnsiConsole.Ask<string>("[cyan]Enter artist name to search:[/]");

        if (string.IsNullOrWhiteSpace(searchName))
        {
            AnsiConsole.MarkupLine("[red]Search term cannot be empty.[/]");
            return;
        }

        var djSets = AnsiConsole.Status()
            .Start("Searching...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("yellow"));
                return context.DjSets
                    .Include(ds => ds.Artist)
                    .Include(ds => ds.Venue)
                    .Where(ds => ds.Artist.DisplayName.Contains(searchName))
                    .OrderByDescending(ds => ds.SetDatetime)
                    .ToList();
            });

        if (!djSets.Any())
        {
            AnsiConsole.MarkupLine($"[yellow]No DJ sets found for artist containing '{searchName}'.[/]");
            return;
        }

        DisplayDjSetsTable(djSets, $"Found {djSets.Count} DJ Set(s) for '{searchName}'");
    }

    static void SearchDjSetsByVenue(AppDbContext context)
    {
        var searchName = AnsiConsole.Ask<string>("[cyan]Enter venue name to search:[/]");

        if (string.IsNullOrWhiteSpace(searchName))
        {
            AnsiConsole.MarkupLine("[red]Search term cannot be empty.[/]");
            return;
        }

        var djSets = AnsiConsole.Status()
            .Start("Searching...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("yellow"));
                return context.DjSets
                    .Include(ds => ds.Artist)
                    .Include(ds => ds.Venue)
                    .Where(ds => ds.Venue != null && ds.Venue.Name != null && ds.Venue.Name.Contains(searchName))
                    .OrderByDescending(ds => ds.SetDatetime)
                    .ToList();
            });

        if (!djSets.Any())
        {
            AnsiConsole.MarkupLine($"[yellow]No DJ sets found for venue containing '{searchName}'.[/]");
            return;
        }

        DisplayDjSetsTable(djSets, $"Found {djSets.Count} DJ Set(s) at '{searchName}'");
    }

    static void SearchDjSetsByDateRange(AppDbContext context)
    {
        var startInput = AnsiConsole.Ask<string>("[cyan]Enter start date (yyyy-MM-dd) or press Enter to skip:[/]", string.Empty);
        DateTime? startDate = null;
        if (!string.IsNullOrWhiteSpace(startInput) && DateTime.TryParse(startInput, out var start))
        {
            startDate = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        }

        var endInput = AnsiConsole.Ask<string>("[cyan]Enter end date (yyyy-MM-dd) or press Enter to skip:[/]", string.Empty);
        DateTime? endDate = null;
        if (!string.IsNullOrWhiteSpace(endInput) && DateTime.TryParse(endInput, out var end))
        {
            endDate = DateTime.SpecifyKind(end.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);
        }

        var djSets = AnsiConsole.Status()
            .Start("Searching...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("yellow"));
            
                var query = context.DjSets
                    .Include(ds => ds.Artist)
                    .Include(ds => ds.Venue)
                    .AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(ds => ds.SetDatetime >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(ds => ds.SetDatetime <= endDate.Value);
                }

                return query.OrderByDescending(ds => ds.SetDatetime).ToList();
            });

        if (!djSets.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No DJ sets found in the specified date range.[/]");
            return;
        }

        var dateRangeText = startDate.HasValue && endDate.HasValue
            ? $"from {startDate.Value:yyyy-MM-dd} to {endDate.Value:yyyy-MM-dd}"
            : startDate.HasValue ? $"from {startDate.Value:yyyy-MM-dd}"
            : endDate.HasValue ? $"until {endDate.Value:yyyy-MM-dd}"
            : "all dates";

        DisplayDjSetsTable(djSets, $"Found {djSets.Count} DJ Set(s) {dateRangeText}");
    }

    static void ViewAllDjSets(AppDbContext context)
    {
        var djSets = AnsiConsole.Status()
            .Start("Loading DJ sets...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("yellow"));
                return context.DjSets
                    .Include(ds => ds.Artist)
                    .Include(ds => ds.Venue)
                    .OrderByDescending(ds => ds.SetDatetime)
                    .ToList();
            });

        if (!djSets.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No DJ sets found in the database.[/]");
            return;
        }

        DisplayDjSetsTable(djSets, $"All DJ Sets ({djSets.Count} total)");
    }

    static void SearchSongsByTitle(AppDbContext context)
    {
        var searchTitle = AnsiConsole.Ask<string>("[cyan]Enter song title to search:[/]");

        if (string.IsNullOrWhiteSpace(searchTitle))
        {
            AnsiConsole.MarkupLine("[red]Search term cannot be empty.[/]");
            return;
        }

        var songs = AnsiConsole.Status()
            .Start("Searching...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("yellow"));
                return context.Songs
                    .Include(s => s.SongArtists)
                    .ThenInclude(sa => sa.Artist)
                    .Where(s => s.Title.Contains(searchTitle))
                    .ToList();
            });

        if (!songs.Any())
        {
            AnsiConsole.MarkupLine($"[yellow]No songs found with title containing '{searchTitle}'.[/]");
            return;
        }

        DisplaySongsTable(songs, $"Found {songs.Count} Song(s) matching '{searchTitle}'");
    }

    static void SearchSongsByArtist(AppDbContext context)
    {
        var searchName = AnsiConsole.Ask<string>("[cyan]Enter artist name to search:[/]");

        if (string.IsNullOrWhiteSpace(searchName))
        {
            AnsiConsole.MarkupLine("[red]Search term cannot be empty.[/]");
            return;
        }

        var songs = AnsiConsole.Status()
            .Start("Searching...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("yellow"));
                return context.Songs
                    .Include(s => s.SongArtists)
                    .ThenInclude(sa => sa.Artist)
                    .Where(s => s.SongArtists.Any(sa => sa.Artist.DisplayName.Contains(searchName)))
                    .ToList();
            });

        if (!songs.Any())
        {
            AnsiConsole.MarkupLine($"[yellow]No songs found for artist containing '{searchName}'.[/]");
            return;
        }

        DisplaySongsTable(songs, $"Found {songs.Count} Song(s) by '{searchName}'");
    }

    static void SearchSongsByGenre(AppDbContext context)
    {
        var searchGenre = AnsiConsole.Ask<string>("[cyan]Enter genre to search:[/]");

        if (string.IsNullOrWhiteSpace(searchGenre))
        {
            AnsiConsole.MarkupLine("[red]Search term cannot be empty.[/]");
            return;
        }

        var songs = AnsiConsole.Status()
            .Start("Searching...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("yellow"));
                return context.Songs
                    .Include(s => s.SongArtists)
                    .ThenInclude(sa => sa.Artist)
                    .Where(s => s.Genre != null && s.Genre.Contains(searchGenre))
                    .ToList();
            });

        if (!songs.Any())
        {
            AnsiConsole.MarkupLine($"[yellow]No songs found with genre containing '{searchGenre}'.[/]");
            return;
        }

        DisplaySongsTable(songs, $"Found {songs.Count} {searchGenre} Song(s)");
    }

    static void ViewDjSetDetails(AppDbContext context)
    {
        var setId = AnsiConsole.Ask<int>("[cyan]Enter DJ set ID:[/]");

        var djSet = AnsiConsole.Status()
            .Start("Loading DJ set details...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("yellow"));
                return context.DjSets
                    .Include(ds => ds.Artist)
                    .Include(ds => ds.Venue)
                    .Include(ds => ds.SetSongs)
                    .ThenInclude(ss => ss.Song)
                    .ThenInclude(s => s.SongArtists)
                    .ThenInclude(sa => sa.Artist)
                    .FirstOrDefault(ds => ds.DjSetId == setId);
            });

        if (djSet == null)
        {
            AnsiConsole.MarkupLine($"[red]DJ set with ID {setId} not found.[/]");
            return;
        }

        // Create details panel
        var detailsGrid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn();

        detailsGrid.AddRow("[bold yellow]ID:[/]", $"[cyan]{djSet.DjSetId}[/]");
        detailsGrid.AddRow("[bold yellow]Title:[/]", $"[white]{djSet.Title ?? "N/A"}[/]");
        detailsGrid.AddRow("[bold yellow]Artist:[/]", $"[green]{djSet.Artist.DisplayName}[/]");
        detailsGrid.AddRow("[bold yellow]Date/Time:[/]", $"[cyan]{(djSet.SetDatetime.HasValue ? djSet.SetDatetime.Value.ToString("yyyy-MM-dd HH:mm") : "N/A")}[/]");
        detailsGrid.AddRow("[bold yellow]Duration:[/]", $"[cyan]{(djSet.DurationMinutes.HasValue ? $"{djSet.DurationMinutes.Value} minutes" : "N/A")}[/]");
        detailsGrid.AddRow("[bold yellow]Venue:[/]", $"[magenta]{djSet.Venue?.Name ?? "N/A"}[/]");
        detailsGrid.AddRow("[bold yellow]Source URL:[/]", $"[dim]{djSet.SourceUrl ?? "N/A"}[/]");

        var panel = new Panel(detailsGrid)
        {
            Header = new PanelHeader($"DJ Set Details", Justify.Center),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Cyan1)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        if (djSet.SetSongs.Any())
        {
            var tracklistTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .AddColumn(new TableColumn("[yellow]#[/]").Centered())
                .AddColumn(new TableColumn("[yellow]Timestamp[/]").Centered())
                .AddColumn(new TableColumn("[yellow]Title[/]"))
                .AddColumn(new TableColumn("[yellow]Artist(s)[/]"));

            int trackNumber = 1;
            foreach (var setSong in djSet.SetSongs.OrderBy(ss => ss.TimestampInSetSeconds ?? int.MaxValue))
            {
                var artists = string.Join(", ", setSong.Song.SongArtists.Select(sa => sa.Artist.DisplayName));
                var timestamp = setSong.TimestampInSetSeconds.HasValue
                    ? TimeSpan.FromSeconds(setSong.TimestampInSetSeconds.Value).ToString(@"mm\:ss")
                    : "---";

                tracklistTable.AddRow(
                    $"[dim]{trackNumber}[/]",
                    $"[cyan]{timestamp}[/]",
                    $"[white]{setSong.Song.Title}[/]",
                    $"[green]{artists}[/]"
                );
                trackNumber++;
            }

            var tracklistPanel = new Panel(tracklistTable)
            {
                Header = new PanelHeader($"Tracklist ({djSet.SetSongs.Count} songs)", Justify.Left),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green)
            };

            AnsiConsole.Write(tracklistPanel);
        }
        else
        {
            AnsiConsole.MarkupLine("[dim]No songs in this set.[/]");
        }
    }

    static void ViewSetAnalytics(AppDbContext context)
    {
        var setId = AnsiConsole.Ask<int>("[cyan]Enter DJ set ID:[/]");

        var djSet = AnsiConsole.Status()
            .Start("Loading analytics...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("yellow"));
                return context.DjSets
                    .Include(ds => ds.Artist)
                    .Include(ds => ds.SetAnalytics)
                    .FirstOrDefault(ds => ds.DjSetId == setId);
            });

        if (djSet == null)
        {
            AnsiConsole.MarkupLine($"[red]DJ set with ID {setId} not found.[/]");
            return;
        }

        var setInfo = $"[bold white]{djSet.Title ?? "N/A"}[/] by [green]{djSet.Artist.DisplayName}[/]";

        if (djSet.SetAnalytics == null)
        {
            var noDataPanel = new Panel(
                new Markup($"{setInfo}\n\n[yellow]No analytics data available for this set.[/]"))
            {
                Header = new PanelHeader("DJ Set Analytics", Justify.Center),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Yellow)
            };
            AnsiConsole.Write(noDataPanel);
            return;
        }

        var analyticsGrid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn();

        analyticsGrid.AddRow("[bold yellow]Set:[/]", setInfo);
        analyticsGrid.AddEmptyRow();
        analyticsGrid.AddRow("[bold cyan]Tickets Sold:[/]", $"[white]{djSet.SetAnalytics.TicketsSold?.ToString("N0") ?? "N/A"}[/]");
        analyticsGrid.AddRow("[bold cyan]Attendance:[/]", $"[white]{djSet.SetAnalytics.AttendanceCount?.ToString("N0") ?? "N/A"}[/]");
        analyticsGrid.AddRow("[bold green]Gross Revenue:[/]", $"[white]{(djSet.SetAnalytics.GrossRevenue.HasValue ? $"${djSet.SetAnalytics.GrossRevenue.Value:N2}" : "N/A")}[/]");
        analyticsGrid.AddRow("[bold magenta]Stream Count:[/]", $"[white]{djSet.SetAnalytics.StreamCount?.ToString("N0") ?? "N/A"}[/]");
        analyticsGrid.AddRow("[bold red]Like Count:[/]", $"[white]{djSet.SetAnalytics.LikeCount?.ToString("N0") ?? "N/A"}[/]");

        var panel = new Panel(analyticsGrid)
        {
            Header = new PanelHeader("DJ Set Analytics", Justify.Center),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Cyan1)
        };

        AnsiConsole.Write(panel);
    }

    static void DisplayDjSetsTable(List<DjSet> djSets, string title)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .AddColumn(new TableColumn("[yellow]ID[/]").Centered())
            .AddColumn(new TableColumn("[yellow]Title[/]"))
            .AddColumn(new TableColumn("[yellow]Artist[/]"))
            .AddColumn(new TableColumn("[yellow]Date[/]").Centered())
            .AddColumn(new TableColumn("[yellow]Duration[/]").Centered())
            .AddColumn(new TableColumn("[yellow]Venue[/]"));

        foreach (var djSet in djSets)
        {
            table.AddRow(
                $"[cyan]{djSet.DjSetId}[/]",
                $"[white]{djSet.Title ?? "Untitled"}[/]",
                $"[green]{djSet.Artist.DisplayName}[/]",
                $"[cyan]{(djSet.SetDatetime.HasValue ? djSet.SetDatetime.Value.ToString("yyyy-MM-dd HH:mm") : "N/A")}[/]",
                $"[yellow]{(djSet.DurationMinutes.HasValue ? $"{djSet.DurationMinutes.Value} min" : "N/A")}[/]",
                $"[magenta]{djSet.Venue?.Name ?? "N/A"}[/]"
            );
        }

        var panel = new Panel(table)
        {
            Header = new PanelHeader($"{title}", Justify.Left),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Cyan1)
        };

        AnsiConsole.Write(panel);
    }

    static void DisplaySongsTable(List<Song> songs, string title)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green)
            .AddColumn(new TableColumn("[yellow]ID[/]").Centered())
            .AddColumn(new TableColumn("[yellow]Title[/]"))
            .AddColumn(new TableColumn("[yellow]Artist(s)[/]"))
            .AddColumn(new TableColumn("[yellow]Genre[/]").Centered())
            .AddColumn(new TableColumn("[yellow]BPM[/]").Centered())
            .AddColumn(new TableColumn("[yellow]Duration[/]").Centered())
            .AddColumn(new TableColumn("[yellow]Release Date[/]").Centered());

        foreach (var song in songs)
        {
            var artists = string.Join(", ", song.SongArtists.Select(sa => sa.Artist.DisplayName));
            table.AddRow(
                $"[cyan]{song.SongId}[/]",
                $"[white]{song.Title}[/]",
                $"[green]{artists}[/]",
                $"[magenta]{song.Genre ?? "N/A"}[/]",
                $"[yellow]{song.Bpm?.ToString() ?? "N/A"}[/]",
                $"[cyan]{(song.DurationSeconds.HasValue ? TimeSpan.FromSeconds(song.DurationSeconds.Value).ToString(@"mm\:ss") : "N/A")}[/]",
                $"[cyan]{(song.ReleaseDate.HasValue ? song.ReleaseDate.Value.ToString("yyyy-MM-dd") : "N/A")}[/]"
            );
        }

        var panel = new Panel(table)
        {
            Header = new PanelHeader($"{title}", Justify.Left),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green)
        };

        AnsiConsole.Write(panel);
    }

    static void AddDjSet(AppDbContext context)
    {
        var rule = new Rule("[cyan]Add New DJ Set[/]")
        {
            Justification = Justify.Left
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        // Get or create artist for the DJ set
        AnsiConsole.MarkupLine("[bold yellow]DJ Set Artist[/]");
        var artist = GetOrCreateArtist(context);
        AnsiConsole.WriteLine();

        // Get DJ set basic information
        AnsiConsole.MarkupLine("[bold yellow]DJ Set Information[/]");
        var title = AnsiConsole.Ask<string>("[cyan]Enter DJ set title:[/]");

        var dateInput = AnsiConsole.Ask<string>("[cyan]Enter set date and time (yyyy-MM-dd HH:mm) or press Enter to skip:[/]", string.Empty);
        DateTime? setDateTime = null;
        if (!string.IsNullOrWhiteSpace(dateInput) && DateTime.TryParse(dateInput, out var parsedDate))
        {
            setDateTime = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        }

        var durationMinutes = AnsiConsole.Ask<int?>("[cyan]Enter duration in minutes (or 0 to skip):[/]", 0);
        if (durationMinutes == 0) durationMinutes = null;

        var sourceUrl = AnsiConsole.Ask<string>("[cyan]Enter source URL (or press Enter to skip):[/]", string.Empty);
        if (string.IsNullOrWhiteSpace(sourceUrl)) sourceUrl = null;

        AnsiConsole.WriteLine();

        // Get or create venue (optional)
        AnsiConsole.MarkupLine("[bold yellow]Venue Information[/]");
        Venue? venue = null;
        if (AnsiConsole.Confirm("Add venue?"))
        {
            venue = GetOrCreateVenue(context);
        }

        // Create the DJ set
        var djSet = new DjSet
        {
            Artist = artist,
            Title = title,
            SetDatetime = setDateTime,
            DurationMinutes = durationMinutes,
            SourceUrl = sourceUrl,
            Venue = venue
        };

        AnsiConsole.Status()
            .Start("Saving DJ set...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("green"));
                context.DjSets.Add(djSet);
                context.SaveChanges();
            });

        AnsiConsole.WriteLine();

        // Add songs to the set
        AnsiConsole.MarkupLine("[bold yellow]Add Songs to Set[/]");
        AddSongsToSet(context, djSet);

        AnsiConsole.WriteLine();

        // Optionally add analytics
        AnsiConsole.MarkupLine("[bold yellow]Analytics (Optional)[/]");
        if (AnsiConsole.Confirm("Add analytics data?", false))
        {
            AddSetAnalytics(context, djSet);
        }

        context.SaveChanges();
    }

    static Artist GetOrCreateArtist(AppDbContext context)
    {
        var searchName = AnsiConsole.Ask<string>("[cyan]Search for existing artist by name (or press Enter to create new):[/]", string.Empty);

        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var existingArtists = context.Artists
                .Where(a => a.DisplayName.Contains(searchName))
                .ToList();

            if (existingArtists.Any())
            {
                var choices = existingArtists
                    .Select(a => $"{Markup.Escape(a.DisplayName)} ({Markup.Escape(a.Country ?? "N/A")})")
                    .ToList();
                var createNewOption = Markup.Escape("[Create new artist]");
                choices.Add(createNewOption);

                var selected = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]Found artists - select one:[/]")
                        .AddChoices(choices));

                if (selected != createNewOption)
                {
                    var index = choices.IndexOf(selected);
                    return existingArtists[index];
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]No artists found. Creating new...[/]");
            }
        }

        // Create new artist
        AnsiConsole.MarkupLine("[dim]--- Create New Artist ---[/]");
        var displayName = AnsiConsole.Ask<string>("[cyan]Enter artist display name:[/]", "Unknown Artist");

        var country = AnsiConsole.Ask<string>("[cyan]Enter country (or press Enter to skip):[/]", string.Empty);
        if (string.IsNullOrWhiteSpace(country)) country = null;

        var newArtist = new Artist
        {
            DisplayName = displayName,
            Country = country
        };

        context.Artists.Add(newArtist);
        context.SaveChanges();

        AnsiConsole.MarkupLine($"[green]Created artist: {displayName}[/]");

        return newArtist;
    }

    static Venue? GetOrCreateVenue(AppDbContext context)
    {
        var searchName = AnsiConsole.Ask<string>("[cyan]Search for existing venue by name (or press Enter to create new):[/]", string.Empty);

        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var existingVenues = context.Venues
                .Where(v => v.Name != null && v.Name.Contains(searchName))
                .ToList();

            if (existingVenues.Any())
            {
                var choices = existingVenues
                    .Select(v => $"{Markup.Escape(v.Name)} - Capacity: {v.Capacity ?? 0}")
                    .ToList();
                var createNewOption = Markup.Escape("[Create new venue]");
                choices.Add(createNewOption);

                var selected = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]Found venues - select one:[/]")
                        .AddChoices(choices));

                if (selected != createNewOption)
                {
                    var index = choices.IndexOf(selected);
                    return existingVenues[index];
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]No venues found. Creating new...[/]");
            }
        }

        // Create new venue
        AnsiConsole.MarkupLine("[dim]--- Create New Venue ---[/]");
        var name = AnsiConsole.Ask<string>("[cyan]Enter venue name:[/]");

        var capacity = AnsiConsole.Ask<int?>("[cyan]Enter capacity (or 0 to skip):[/]", 0);
        if (capacity == 0) capacity = null;

        var address = AnsiConsole.Ask<string>("[cyan]Enter address (or press Enter to skip):[/]", string.Empty);
        if (string.IsNullOrWhiteSpace(address)) address = null;

        var newVenue = new Venue
        {
            Name = name,
            Capacity = capacity,
            Address = address
        };

        context.Venues.Add(newVenue);
        context.SaveChanges();

        AnsiConsole.MarkupLine($"[green]Created venue: {name}[/]");

        return newVenue;
    }

    static void AddSongsToSet(AppDbContext context, DjSet djSet)
    {
        while (true)
        {
            if (!AnsiConsole.Confirm("Add a song to the set?", false)) break;

            var searchTitle = AnsiConsole.Ask<string>("[cyan]Search for existing song by title (or press Enter to create new):[/]", string.Empty);

            Song? song = null;

            if (!string.IsNullOrWhiteSpace(searchTitle))
            {
                var existingSongs = context.Songs
                    .Include(s => s.SongArtists)
                    .ThenInclude(sa => sa.Artist)
                    .Where(s => s.Title.Contains(searchTitle))
                    .ToList();

                if (existingSongs.Any())
                {
                    var choices = existingSongs
                        .Select(s => $"{Markup.Escape(s.Title)} by {string.Join(", ", s.SongArtists.Select(sa => Markup.Escape(sa.Artist.DisplayName)))}")
                        .ToList();
                    var createNewOption = Markup.Escape("[Create new song]");
                    choices.Add(createNewOption);

                    var selected = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[green]Found songs - select one:[/]")
                            .AddChoices(choices));

                    if (selected != createNewOption)
                    {
                        var index = choices.IndexOf(selected);
                        song = existingSongs[index];
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]No songs found. Creating new...[/]");
                }
            }

            // Create new song if not selected
            if (song == null)
            {
                song = CreateNewSong(context);
            }

            // Add song to set with timestamp
            var timestamp = AnsiConsole.Ask<int?>("[cyan]Enter timestamp in set (seconds) or 0 to skip:[/]", 0);
            if (timestamp == 0) timestamp = null;

            var setSong = new SetSong
            {
                Song = song,
                DjSet = djSet,
                TimestampInSetSeconds = timestamp
            };

            context.SetSongs.Add(setSong);
            context.SaveChanges();

            AnsiConsole.MarkupLine($"[green]Added '{song.Title}' to the set![/]");
        }
    }

    static Song CreateNewSong(AppDbContext context)
    {
        AnsiConsole.MarkupLine("[dim]--- Create New Song ---[/]");
        var title = AnsiConsole.Ask<string>("[cyan]Enter song title:[/]", "Untitled");

        var releaseDateStr = AnsiConsole.Ask<string>("[cyan]Enter release date (yyyy-MM-dd) or press Enter to skip:[/]", string.Empty);
        DateOnly? releaseDate = null;
        if (DateOnly.TryParse(releaseDateStr, out var date))
        {
            releaseDate = date;
        }

        var duration = AnsiConsole.Ask<int?>("[cyan]Enter duration in seconds (or 0 to skip):[/]", 0);
        if (duration == 0) duration = null;

        var genre = AnsiConsole.Ask<string>("[cyan]Enter genre (or press Enter to skip):[/]", string.Empty);
        if (string.IsNullOrWhiteSpace(genre)) genre = null;

        var bpm = AnsiConsole.Ask<int?>("[cyan]Enter BPM (or 0 to skip):[/]", 0);
        if (bpm == 0) bpm = null;

        var song = new Song
        {
            Title = title,
            ReleaseDate = releaseDate,
            DurationSeconds = duration,
            Genre = genre,
            Bpm = bpm
        };

        context.Songs.Add(song);
        context.SaveChanges();

        AnsiConsole.MarkupLine($"[green]Created song: {title}[/]");

        // Add artists to song
        AnsiConsole.MarkupLine("\n[dim]--- Add Artists to Song ---[/]");
        while (true)
        {
            if (!AnsiConsole.Confirm("Add an artist to this song?", false)) break;

            var artist = GetOrCreateArtist(context);

            var songArtist = new SongArtist
            {
                Song = song,
                Artist = artist
            };

            context.SongArtists.Add(songArtist);
            context.SaveChanges();

            AnsiConsole.MarkupLine($"[green]Added artist '{artist.DisplayName}' to song![/]");
        }

        return song;
    }

    static void AddSetAnalytics(AppDbContext context, DjSet djSet)
    {
        var ticketsSold = AnsiConsole.Ask<int?>("[cyan]Enter tickets sold (or 0 to skip):[/]", 0);
        if (ticketsSold == 0) ticketsSold = null;

        var attendance = AnsiConsole.Ask<int?>("[cyan]Enter attendance count (or 0 to skip):[/]", 0);
        if (attendance == 0) attendance = null;

        var revenue = AnsiConsole.Ask<int?>("[cyan]Enter gross revenue (or 0 to skip):[/]", 0);
        if (revenue == 0) revenue = null;

        var streams = AnsiConsole.Ask<int?>("[cyan]Enter stream count (or 0 to skip):[/]", 0);
        if (streams == 0) streams = null;

        var likes = AnsiConsole.Ask<int?>("[cyan]Enter like count (or 0 to skip):[/]", 0);
        if (likes == 0) likes = null;

        var analytics = new SetAnalytics
        {
            DjSet = djSet,
            TicketsSold = ticketsSold,
            AttendanceCount = attendance,
            GrossRevenue = revenue,
            StreamCount = streams,
            LikeCount = likes
        };

        context.SetAnalytics.Add(analytics);
        context.SaveChanges();

        AnsiConsole.MarkupLine("[green]Analytics added successfully![/]");
    }

    static void UpdateDjSet(AppDbContext context)
    {
        var rule = new Rule("[cyan]Update DJ Set[/]")
        {
            Justification = Justify.Left
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        var setId = AnsiConsole.Ask<int>("[cyan]Enter DJ set ID to update:[/]");

        var djSet = context.DjSets
            .Include(ds => ds.Artist)
            .Include(ds => ds.Venue)
            .FirstOrDefault(ds => ds.DjSetId == setId);

        if (djSet == null)
        {
            AnsiConsole.MarkupLine($"[red]No DJ set found with ID {setId}.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"\n[yellow]Editing DJ Set:[/] [white]{djSet.Title ?? "Untitled"}[/] by [green]{djSet.Artist.DisplayName}[/]\n");

        var newTitle = AnsiConsole.Ask<string>($"[cyan]Enter new title (or press Enter to keep '[white]{djSet.Title}[/]'):[/]", djSet.Title ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(newTitle))
            djSet.Title = newTitle;

        var currentDate = djSet.SetDatetime.HasValue ? djSet.SetDatetime.Value.ToString("yyyy-MM-dd HH:mm") : "N/A";
        var dateInput = AnsiConsole.Ask<string>($"[cyan]Enter new date/time (yyyy-MM-dd HH:mm) or press Enter to keep '{currentDate}':[/]", string.Empty);
        if (!string.IsNullOrWhiteSpace(dateInput) && DateTime.TryParse(dateInput, out var parsedDate))
            djSet.SetDatetime = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

        var durationInput = AnsiConsole.Ask<string>($"[cyan]Enter new duration (minutes) or press Enter to keep {djSet.DurationMinutes}:[/]", string.Empty);
        if (int.TryParse(durationInput, out int newDuration))
            djSet.DurationMinutes = newDuration;

        if (AnsiConsole.Confirm("Change venue?", false))
            djSet.Venue = GetOrCreateVenue(context);

        if (AnsiConsole.Confirm("Change source URL?", false))
        {
            djSet.SourceUrl = AnsiConsole.Ask<string>("[cyan]Enter new source URL:[/]");
        }

        AnsiConsole.Status()
            .Start("Saving changes...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("green"));
                context.SaveChanges();
            });

        AnsiConsole.MarkupLine("\n[green]DJ Set updated successfully![/]");
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    static void DeleteDjSet(AppDbContext context)
    {
        var rule = new Rule("[red]Delete DJ Set[/]")
        {
            Justification = Justify.Left
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();

        var setId = AnsiConsole.Ask<int>("[cyan]Enter DJ set ID to delete:[/]");

        var djSet = context.DjSets
            .Include(ds => ds.SetSongs)
            .Include(ds => ds.SetAnalytics)
            .FirstOrDefault(ds => ds.DjSetId == setId);

        if (djSet == null)
        {
            AnsiConsole.MarkupLine($"[red]No DJ set found with ID {setId}.[/]");
            AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
            Console.ReadKey(true);
            return;
        }

        var warningPanel = new Panel(
            new Markup($"[yellow]WARNING[/]\n\nYou are about to delete:\n[white]'{djSet.Title ?? "Untitled"}'[/]\n\nThis action cannot be undone!"))
        {
            Border = BoxBorder.Heavy,
            BorderStyle = new Style(Color.Red)
        };

        AnsiConsole.Write(warningPanel);
        AnsiConsole.WriteLine();

        if (!AnsiConsole.Confirm("[red]Are you sure you want to delete this DJ set?[/]", false))
        {
            AnsiConsole.MarkupLine("[yellow]Deletion canceled.[/]");
            AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
            Console.ReadKey(true);
            return;
        }

        AnsiConsole.Status()
            .Start("Deleting DJ set...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("red"));

                // Remove related data first
                if (djSet.SetSongs != null)
                    context.SetSongs.RemoveRange(djSet.SetSongs);

                if (djSet.SetAnalytics != null)
                    context.SetAnalytics.Remove(djSet.SetAnalytics);

                // Then remove the DJ set
                context.DjSets.Remove(djSet);
                context.SaveChanges();
            });

        AnsiConsole.MarkupLine("\n[green]DJ Set deleted successfully![/]");
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
}



