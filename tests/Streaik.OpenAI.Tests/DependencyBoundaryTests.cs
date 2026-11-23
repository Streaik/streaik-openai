using System.Reflection;
using System.Text.Json.Serialization;
using Streaik.Common.Model.Providers.OpenAI;
using Streaik.Core.Models;
using Microsoft.AspNetCore.Http;
using Streaik.OpenAI.Models;
using Streaik.OpenAI.Models.Realtime;
using Streaik.OpenAI.Model.Providers.OpenAI;
using Streaik.OpenAI.Model.Providers.OpenAI.Live;

namespace Streaik.OpenAI.Tests;

public class DependencyBoundaryTests
{
    [Fact]
    public void Contract_assembly_references_only_platform_assemblies()
    {
        Assert.All(typeof(AudioSpeechRequest).Assembly.GetReferencedAssemblies(), reference =>
            Assert.True(reference.Name!.StartsWith("System.", StringComparison.Ordinal)
                || reference.Name.StartsWith("Microsoft.AspNetCore.", StringComparison.Ordinal), reference.FullName));
    }

    [Fact]
    public void All_attributed_contract_converters_are_owned_by_the_same_assembly()
    {
        var assembly = typeof(AudioSpeechRequest).Assembly;
        var converters = assembly.GetExportedTypes().SelectMany(type =>
            type.GetCustomAttributes<JsonConverterAttribute>()
                .Concat(type.GetProperties().SelectMany(property => property.GetCustomAttributes<JsonConverterAttribute>())));
        Assert.NotEmpty(converters);
        Assert.All(converters, attribute => Assert.Same(assembly, attribute.ConverterType!.Assembly));
        Assert.Same(assembly, typeof(TokenPriceConverter).Assembly);
        Assert.Same(assembly, typeof(ShellEnvironmentUnionConverter).Assembly);
        Assert.Same(assembly, typeof(LiveEventPermissionsJsonConverterFactory).Assembly);
    }

    [Fact]
    public void Multipart_properties_retain_the_existing_ASPNET_types()
    {
        Assert.Equal(typeof(IFormFile), typeof(OpenAITranscriptionRequest).GetProperty(nameof(OpenAITranscriptionRequest.File))!.PropertyType);
        Assert.Equal(typeof(IFormFile[]), typeof(OpenAIImageEditRequest).GetProperty(nameof(OpenAIImageEditRequest.ImageFiles))!.PropertyType);
        Assert.Equal(typeof(IFormFile), typeof(OpenAIImageEditRequest).GetProperty(nameof(OpenAIImageEditRequest.MaskFile))!.PropertyType);
    }
}
