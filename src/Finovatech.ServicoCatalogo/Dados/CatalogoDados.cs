using Finovatech.ServicoCatalogo.Dominio;

namespace Finovatech.ServicoCatalogo.Dados;

public static class CatalogoDados
{
    private static IReadOnlyList<string> GaleriaGpu() => ["/images/gpu.svg", "/images/gpu.svg", "/images/gpu.svg"];
    private static IReadOnlyList<string> GaleriaCpu() => ["/images/cpu.svg", "/images/cpu.svg", "/images/cpu.svg"];
    private static IReadOnlyList<string> GaleriaKbd() => ["/images/keyboard.svg", "/images/keyboard.svg", "/images/keyboard.svg"];
    private static IReadOnlyList<string> GaleriaMouse() => ["/images/mouse.svg", "/images/mouse.svg", "/images/mouse.svg"];
    private static IReadOnlyList<string> GaleriaSsd() => ["/images/ssd.svg", "/images/ssd.svg", "/images/ssd.svg"];
    private static IReadOnlyList<string> GaleriaRam() => ["/images/ram.svg", "/images/ram.svg", "/images/ram.svg"];
    private static IReadOnlyList<string> GaleriaSoft() => ["/images/software.svg", "/images/software.svg", "/images/software.svg"];

    public static readonly IReadOnlyList<Produto> Produtos =
    [
        new(Guid.Parse("11111111-0001-0000-0000-000000000000"),
            "RTX 4080 Super 16GB", "GPUs", 4299m, 5049m, 12,
            "Placa de vídeo NVIDIA RTX 4080 Super 16GB GDDR6X — clock boost 2550 MHz, 320W TDP",
            "/images/gpu.svg", GaleriaGpu()),

        new(Guid.Parse("11111111-0002-0000-0000-000000000000"),
            "RTX 4070 Ti Super", "GPUs", 2999m, 3499m, 8,
            "Placa de vídeo NVIDIA RTX 4070 Ti Super 16GB — ideal para 1440p e ray tracing",
            "/images/gpu.svg", GaleriaGpu()),

        new(Guid.Parse("11111111-0003-0000-0000-000000000000"),
            "RX 7900 XT", "GPUs", 3199m, null, 5,
            "Placa de vídeo AMD RX 7900 XT 20GB GDDR6 — arquitetura RDNA 3",
            "/images/gpu.svg", GaleriaGpu()),

        new(Guid.Parse("11111111-0004-0000-0000-000000000000"),
            "RTX 5090", "GPUs", 8999m, null, 3,
            "Placa de vídeo NVIDIA RTX 5090 32GB GDDR7 — topo de linha para criadores",
            "/images/gpu.svg", GaleriaGpu()),

        new(Guid.Parse("11111111-0005-0000-0000-000000000000"),
            "RTX 4060 Ti", "GPUs", 1799m, 2099m, 20,
            "Placa de vídeo NVIDIA RTX 4060 Ti 8GB — excelente custo-benefício para 1080p",
            "/images/gpu.svg", GaleriaGpu()),

        new(Guid.Parse("22222222-0001-0000-0000-000000000000"),
            "Ryzen 9 7950X", "Processadores", 2799m, null, 7,
            "Processador AMD Ryzen 9 7950X 16-core 4.5GHz — workstation e criação de conteúdo",
            "/images/cpu.svg", GaleriaCpu()),

        new(Guid.Parse("22222222-0002-0000-0000-000000000000"),
            "Intel Core i9-14900K", "Processadores", 2199m, 2799m, 9,
            "Processador Intel Core i9-14900K 24-core 3.2GHz — gaming e multitarefa extrema",
            "/images/cpu.svg", GaleriaCpu()),

        new(Guid.Parse("22222222-0003-0000-0000-000000000000"),
            "Ryzen 7 7700X", "Processadores", 1299m, null, 14,
            "Processador AMD Ryzen 7 7700X 8-core 4.5GHz — equilíbrio perfeito para gaming",
            "/images/cpu.svg", GaleriaCpu()),

        new(Guid.Parse("22222222-0004-0000-0000-000000000000"),
            "Intel Core i7-14700K", "Processadores", 1599m, 1899m, 11,
            "Processador Intel Core i7-14700K 20-core 3.4GHz — potência para criar e jogar",
            "/images/cpu.svg", GaleriaCpu()),

        new(Guid.Parse("22222222-0005-0000-0000-000000000000"),
            "Ryzen 5 7600X", "Processadores", 899m, null, 22,
            "Processador AMD Ryzen 5 7600X 6-core 4.7GHz — entrada no AM5 com ótimo preço",
            "/images/cpu.svg", GaleriaCpu()),

        new(Guid.Parse("33333333-0001-0000-0000-000000000000"),
            "MX Keys S", "Periféricos", 649m, 799m, 30,
            "Teclado sem fio Logitech MX Keys S — retroiluminação adaptativa, para Mac/Windows",
            "/images/keyboard.svg", GaleriaKbd()),

        new(Guid.Parse("33333333-0002-0000-0000-000000000000"),
            "Logitech G Pro X Superlight 2", "Periféricos", 449m, null, 18,
            "Mouse gamer Logitech G Pro X Superlight 2 — 60g, sensor HERO 2 25K",
            "/images/mouse.svg", GaleriaMouse()),

        new(Guid.Parse("33333333-0003-0000-0000-000000000000"),
            "HyperX DDR5 64GB", "Periféricos", 899m, 999m, 10,
            "Kit memória RAM DDR5 64GB 5600MHz CL36 — com heatspreader de alumínio",
            "/images/ram.svg", GaleriaRam()),

        new(Guid.Parse("33333333-0004-0000-0000-000000000000"),
            "Samsung 990 Pro 2TB", "Periféricos", 549m, 699m, 25,
            "SSD NVMe M.2 Samsung 990 Pro 2TB PCIe 4.0 — até 7.450 MB/s leitura",
            "/images/ssd.svg", GaleriaSsd()),

        new(Guid.Parse("33333333-0005-0000-0000-000000000000"),
            "Logitech G502 X Plus", "Periféricos", 399m, null, 15,
            "Mouse gamer sem fio Logitech G502 X Plus — 13 botões, sensor HERO 25K",
            "/images/mouse.svg", GaleriaMouse()),

        new(Guid.Parse("44444444-0001-0000-0000-000000000000"),
            "Windows 11 Pro", "Licenças", 299m, null, 999,
            "Licença digital Windows 11 Pro — ativação imediata, suporte a 2 anos",
            "/images/software.svg", GaleriaSoft()),

        new(Guid.Parse("44444444-0002-0000-0000-000000000000"),
            "Office 365 Family", "Licenças", 399m, 499m, 999,
            "Assinatura anual Microsoft 365 Family — 6 usuários, 1TB OneDrive cada",
            "/images/software.svg", GaleriaSoft()),

        new(Guid.Parse("44444444-0003-0000-0000-000000000000"),
            "Adobe Creative Cloud", "Licenças", 299m, null, 999,
            "Assinatura mensal Adobe Creative Cloud All Apps — Photoshop, Premiere, After Effects",
            "/images/software.svg", GaleriaSoft()),

        new(Guid.Parse("44444444-0004-0000-0000-000000000000"),
            "JetBrains All Products", "Licenças", 499m, 599m, 999,
            "Assinatura anual JetBrains All Products Pack — IntelliJ, Rider, DataGrip e mais",
            "/images/software.svg", GaleriaSoft()),

        new(Guid.Parse("44444444-0005-0000-0000-000000000000"),
            "GitHub Copilot Business", "Licenças", 189m, null, 999,
            "Assinatura mensal GitHub Copilot Business — IA para desenvolvimento no seu IDE",
            "/images/software.svg", GaleriaSoft()),
    ];
}
