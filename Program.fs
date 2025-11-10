open System
open System.IO
open System.Text.RegularExpressions
open Markdig

[<EntryPoint>]
let main argv =
    let markdownFile = "slides.md"

    if not (File.Exists markdownFile) then
        printfn $"Error: {markdownFile} not found!"
        Environment.Exit 1

    let markdown = File.ReadAllText(markdownFile)

    // --- Step 1: Convert Markdown to HTML ---
    let pipeline = 
        MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build()

    let htmlContent = Markdown.ToHtml(markdown, pipeline)

    // --- Step 2: Automatically convert F# code blocks to runnable Try .NET ---
    // Matches ```fsharp ... ```
    let pattern = @"<pre><code class=""language-fsharp"">(.*?)</code></pre>"
    let slidesWithRunnable =
        Regex.Replace(htmlContent, pattern, fun (m: Match) ->
            let code = m.Groups.[1].Value
                        .Replace("&lt;", "<")
                        .Replace("&gt;", ">")
                        .Replace("&amp;", "&")
                        .Replace("&quot;", "\"")
                        .Replace("&apos;", "'")
            // Wrap in dotnet-try-dotnet
            $"""<dotnet-try-dotnet language="fsharp" code="{code.Replace("\"", "&quot;")}" />"""
        )

    // --- Step 3: Replace '---' with new slide sections ---
    let slidesHtml = slidesWithRunnable.Replace("---", "</section><section>")

    // --- Step 4: HTML template with Reveal.js and Try .NET script ---
    let htmlTemplate = $"""
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<title>F# Slides</title>
<link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/reveal.js/5.0.4/reveal.min.css'>
<link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/reveal.js/5.0.4/theme/black.min.css'>
<script src='https://cdnjs.cloudflare.com/ajax/libs/reveal.js/5.0.4/reveal.min.js'></script>
<script src='https://try.dot.net/scripts/trydotnet.js'></script>
<style>
.reveal pre code {{ font-size: 1.1em; }}
</style>
</head>
<body>
<div class='reveal'>
<div class='slides'>
<section>
{slidesHtml}
</section>
</div>
</div>
<script>
Reveal.initialize({{ hash: true, transition: 'fade' }});
</script>
</body>
</html>
"""

    let outputFile = "slides.html"
    File.WriteAllText(outputFile, htmlTemplate)
    printfn $"✅ Slides generated: {Path.GetFullPath(outputFile)}"

    0
