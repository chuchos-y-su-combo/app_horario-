using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace ApplicationSchedule.Infrastructure.Services;

/// <summary>
/// Analiza hojas de Excel con texto libre o formato tabular y extrae bloques de disponibilidad.
/// Utilizado por <see cref="CurriculoDocenteService"/> para convertir hojas en entidades de disponibilidad.
/// </summary>
internal static class DisponibilidadExcelParser
{
    private const string HoraInicioDia = "07:00";
    private const string HoraFinDia = "22:30";
    private const string HoraFinSinNoche = "18:00";
    private const string HoraFinManana = "12:00";
    private const string HoraInicioNoche = "18:00";

    /// <summary>
    /// Lee una hoja y devuelve una lista de elementos de disponibilidad detectados.
    /// Adapta múltiples formatos conocidos de la coordinación.
    /// </summary>
    public static List<DisponibilidadExcelItem> LeerHoja(IXLWorksheet worksheet)
    {
        return TieneFormatoNormalizado(worksheet)
            ? LeerFormatoNormalizado(worksheet)
            : LeerFormatoActualCoordinacion(worksheet);
    }

    /// <summary>
    /// Lee la hoja vertical "Mi Disponibilidad" (un único docente por archivo).
    /// Estructura: B6=nombre, B7=tipo contrato, B11–B15=materias, B18=texto disponibilidad.
    /// </summary>
    public static MiDisponibilidadExcelItem LeerHojaMiDisponibilidad(IXLWorksheet worksheet)
    {
        var mensajes = new List<string>();

        try
        {
            string nombre = LeerCeldaSegura(worksheet, 6, 2, mensajes, "B6 nombre");

            string cellB7 = LeerCeldaSegura(worksheet, 7, 2, mensajes, "B7 tipoContrato");
            // Strip control characters (newlines, tabs) that Trim() alone doesn't remove
            string cellB7Clean = new string(cellB7.Where(c => !char.IsControl(c)).ToArray()).Trim();
            string tipoContratoRaw = NormalizarTexto(cellB7Clean);
            string tipoContrato = (tipoContratoRaw == "TC"
                || tipoContratoRaw.StartsWith("TC")
                || tipoContratoRaw == "TIEMPO COMPLETO"
                || tipoContratoRaw == "PLANTA")
                ? "TC" : "TP";
            mensajes.Add($"[B7] Tipo contrato interpretado: '{tipoContrato}' (raw: '{cellB7Clean}')");

            var materias = new List<string>();
            for (int fila = 11; fila <= 15; fila++)
            {
                string materia = LeerCeldaSegura(worksheet, fila, 2, mensajes, $"B{fila} materia");
                if (!string.IsNullOrWhiteSpace(materia))
                    materias.Add(materia);
            }

            string textoDisponibilidad = LeerCeldaSegura(worksheet, 18, 2, mensajes, "B18 disponibilidad");

            List<DisponibilidadBloque> bloques;
            try
            {
                bloques = InterpretarTextoLibre(textoDisponibilidad, mensajes);
            }
            catch (Exception ex)
            {
                mensajes.Add($"[InterpretarTextoLibre] Error: {ex.Message}");
                bloques = new List<DisponibilidadBloque>();
            }

            return new MiDisponibilidadExcelItem
            {
                NombreDocente = nombre,
                TipoContrato = tipoContrato,
                NombresMaterias = materias,
                TextoDisponibilidad = textoDisponibilidad,
                Bloques = bloques,
                Mensajes = mensajes
            };
        }
        catch (Exception ex)
        {
            mensajes.Add($"[LeerHojaMiDisponibilidad] Error crítico: {ex.Message}");
            return new MiDisponibilidadExcelItem { Mensajes = mensajes };
        }
    }

    private static string LeerCeldaSegura(IXLWorksheet worksheet, int fila, int columna, List<string> mensajes, string etiqueta)
    {
        try
        {
            var cell = worksheet.Cell(fila, columna);
            string valor = cell.IsEmpty() ? string.Empty : cell.GetString().Trim();
            mensajes.Add($"[{etiqueta}] = '{valor}'");
            return valor;
        }
        catch (Exception ex)
        {
            mensajes.Add($"[{etiqueta}] Error al leer celda ({fila},{columna}): {ex.Message}");
            try { return worksheet.Cell(fila, columna).Value.ToString()?.Trim() ?? string.Empty; }
            catch { return string.Empty; }
        }
    }

    private static bool TieneFormatoNormalizado(IXLWorksheet worksheet)
    {
        string a1 = NormalizarTexto(worksheet.Cell(1, 1).GetString());
        string b1 = NormalizarTexto(worksheet.Cell(1, 2).GetString());
        string c1 = NormalizarTexto(worksheet.Cell(1, 3).GetString());
        string d1 = NormalizarTexto(worksheet.Cell(1, 4).GetString());

        return a1 == "DOCENTE"
            && b1 == "DIA"
            && c1 == "HORA INICIO"
            && d1 == "HORA FIN";
    }

    private static List<DisponibilidadExcelItem> LeerFormatoNormalizado(IXLWorksheet worksheet)
    {
        var resultado = new List<DisponibilidadExcelItem>();

        int ultimaFila = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (int fila = 2; fila <= ultimaFila; fila++)
        {
            string docente = worksheet.Cell(fila, 1).GetString().Trim();
            string dia = worksheet.Cell(fila, 2).GetString().Trim();
            string horaInicio = worksheet.Cell(fila, 3).GetString().Trim();
            string horaFin = worksheet.Cell(fila, 4).GetString().Trim();

            if (string.IsNullOrWhiteSpace(docente))
                continue;

            int diaSemana = ConvertirDiaTextoANumero(dia);

            var item = resultado.FirstOrDefault(r =>
                NormalizarTexto(r.NombreDocente) == NormalizarTexto(docente));

            if (item is null)
            {
                item = new DisponibilidadExcelItem
                {
                    NombreDocente = docente,
                    TextoOriginal = "Formato normalizado"
                };
                resultado.Add(item);
            }

            if (diaSemana == 0)
            {
                item.Mensajes.Add($"Fila {fila}: día no válido: {dia}");
                continue;
            }

            if (!EsHoraValida(horaInicio) || !EsHoraValida(horaFin))
            {
                item.Mensajes.Add($"Fila {fila}: hora inválida.");
                continue;
            }

            item.Bloques.Add(new DisponibilidadBloque(diaSemana, horaInicio, horaFin));
        }

        return resultado;
    }

    private static List<DisponibilidadExcelItem> LeerFormatoActualCoordinacion(IXLWorksheet worksheet)
    {
        var resultado = new List<DisponibilidadExcelItem>();

        int ultimaColumna = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

        for (int columna = 1; columna <= ultimaColumna; columna++)
        {
            string nombreDocente = worksheet.Cell(1, columna).GetString().Trim();
            string textoDisponibilidad = worksheet.Cell(2, columna).GetString().Trim();

            if (string.IsNullOrWhiteSpace(nombreDocente))
                continue;

            var mensajes = new List<string>();
            List<DisponibilidadBloque> bloques = InterpretarTextoLibre(textoDisponibilidad, mensajes);

            resultado.Add(new DisponibilidadExcelItem
            {
                NombreDocente = nombreDocente,
                TextoOriginal = textoDisponibilidad,
                Bloques = bloques,
                Mensajes = mensajes
            });
        }

        return resultado;
    }

    internal static List<DisponibilidadBloque> InterpretarTextoLibre(
        string textoOriginal,
        List<string> mensajes)
    {
        var bloques = new List<DisponibilidadBloque>();

        if (string.IsNullOrWhiteSpace(textoOriginal))
        {
            mensajes.Add("No tiene texto de disponibilidad.");
            return bloques;
        }

        string texto = NormalizarTexto(textoOriginal)
            .Replace("7AMA", "7AM A")
            .Replace("TODO EL DIAM", "TODO EL DIA")
            .Replace("4:0O", "4:00")
            .Replace(" EN EN ", " EN ");

        string finDia = texto.Contains("EVITAR NOCHES") || texto.Contains("EVITAR NOCHE")
            ? HoraFinSinNoche
            : HoraFinDia;

        // ── Patrones globales de rango de días ──────────────────────────────

        if (texto.Contains("TODO EL DIA L A J") ||
            texto.Contains("L A J TODO EL DIA") ||
            texto.Contains("L A J TODO EL DIA Y NOCHE") ||
            texto.Contains("LUNES A JUEVES TODO EL DIA") ||
            texto.Contains("LUNES A JUEVES  TODO EL DIA"))
        {
            AgregarRangoDias(bloques, 1, 4, HoraInicioDia, finDia);
        }

        if (texto.Contains("TODO EL DIA L A V") ||
            texto.Contains("LUNES A VIERNES TODO EL DIA"))
        {
            AgregarRangoDias(bloques, 1, 5, HoraInicioDia, finDia);
        }

        if (texto.Contains("L A MI TODO EL DIA") ||
            texto.Contains("LUNES A MIERCOLES TODO EL DIA"))
        {
            string horaFin = texto.Contains("8PM") ? "20:00" : finDia;
            AgregarRangoDias(bloques, 1, 3, HoraInicioDia, horaFin);
        }

        if (texto.Contains("L Y MARTES TODO EL DIA") ||
            texto.Contains("LUNES Y MARTES TODO EL DIA"))
        {
            AgregarBloque(bloques, 1, HoraInicioDia, finDia);
            AgregarBloque(bloques, 2, HoraInicioDia, finDia);
        }

        if (texto.Contains("JUEVES TODO EL DIA"))
            AgregarBloque(bloques, 4, HoraInicioDia, finDia);

        // ── Patrones de periodo del día ──────────────────────────────────────

        if (texto.Contains("MIERCOLES EN LA MANANA") ||
            texto.Contains("MIERCOLES EN MANANA"))
        {
            AgregarBloque(bloques, 3, HoraInicioDia, HoraFinManana);
        }

        if (texto.Contains("VIERNES EN LA MANANA") ||
            texto.Contains("VIERNES MANANA"))
        {
            AgregarBloque(bloques, 5, HoraInicioDia, HoraFinManana);
        }

        if (texto.Contains("VIERNES") && texto.Contains("NOCHE"))
            AgregarBloque(bloques, 5, HoraInicioNoche, HoraFinDia);

        // ── Patrones específicos de hora ─────────────────────────────────────

        if (texto.Contains("DESPUES DE LAS 6PM"))
            AgregarRangoDias(bloques, 1, 4, "18:00", HoraFinDia);

        if (texto.Contains("DE 7AM A 9AM"))
            AgregarRangoDias(bloques, 1, 5, "07:00", "09:00");

        if (texto.Contains("LUNES A VIERNES DE 7AM A 10AM"))
            AgregarRangoDias(bloques, 1, 5, "07:00", "10:00");

        if (texto.Contains("LUNES A JUEVES DE 6PM A 10PM"))
            AgregarRangoDias(bloques, 1, 4, "18:00", "22:00");

        if (texto.Contains("LUNES Y MIERCOLES DE 6:30 EN ADELANTE"))
        {
            AgregarBloque(bloques, 1, "18:30", HoraFinDia);
            AgregarBloque(bloques, 3, "18:30", HoraFinDia);
        }

        if (texto.Contains("MARTES Y JUEVES DE 4 EN ADELANTE"))
        {
            AgregarBloque(bloques, 2, "16:00", HoraFinDia);
            AgregarBloque(bloques, 4, "16:00", HoraFinDia);
        }

        if (texto.Contains("JUEVES Y VIERNES DESPUES DE LAS 10AM"))
        {
            AgregarBloque(bloques, 4, "10:00", HoraFinDia);
            AgregarBloque(bloques, 5, "10:00", HoraFinDia);
        }

        // ── Análisis línea a línea (captura casos no cubiertos) ──────────────
        InterpretarLineasConDia(textoOriginal, bloques);

        List<DisponibilidadBloque> resultado = bloques
            .GroupBy(b => $"{b.DiaSemana}|{b.HoraInicio}|{b.HoraFin}")
            .Select(g => g.First())
            .OrderBy(b => b.DiaSemana)
            .ThenBy(b => b.HoraInicio)
            .ToList();

        if (resultado.Count == 0)
        {
            mensajes.Add("No se pudo convertir el texto libre a bloques de día y hora. Requiere revisión manual.");
        }

        return resultado;
    }

    /// <summary>
    /// Divide el texto en segmentos manejables: primero en separadores estándar (".","," newline)
    /// y luego sub-divide cada segmento antes de nombres de día concatenados sin separador,
    /// para manejar cadenas como "Lunes 8am a 12m Martes 4pm en adelante".
    /// </summary>
    private static IEnumerable<string> ObtenerSegmentos(string textoOriginal)
    {
        var partes = textoOriginal
            .Replace(".", "\n")
            .Replace(",", "\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (string parte in partes)
        {
            // Sub-dividir antes de nombres de día cuando aparecen en medio de una cadena
            string[] subPartes = Regex.Split(
                parte,
                @"\s+(?=(?:Lunes|Martes|Mi[eé]rcoles|Jueves|Viernes|S[aá]bado)\b)",
                RegexOptions.IgnoreCase
            );

            foreach (string sub in subPartes)
            {
                if (!string.IsNullOrWhiteSpace(sub))
                    yield return sub.Trim();
            }
        }
    }

    /// <summary>
    /// Detecta todos los días de la semana mencionados en un texto normalizado (mayúsculas, sin tildes).
    /// Maneja múltiples días en un mismo segmento ("Lunes y Miércoles", "Martes y jueves").
    /// </summary>
    private static List<int> DetectarDiasEnTexto(string textoNormalizado)
    {
        var dias = new List<int>();
        if (textoNormalizado.Contains("LUNES")) dias.Add(1);
        if (textoNormalizado.Contains("MARTES")) dias.Add(2);
        if (textoNormalizado.Contains("MIERCOLES")) dias.Add(3);
        if (textoNormalizado.Contains("JUEVES")) dias.Add(4);
        if (textoNormalizado.Contains("VIERNES")) dias.Add(5);
        if (textoNormalizado.Contains("SABADO")) dias.Add(6);
        return dias;
    }

    private static void InterpretarLineasConDia(string textoOriginal, List<DisponibilidadBloque> bloques)
    {
        foreach (string segmentoOriginal in ObtenerSegmentos(textoOriginal))
        {
            string parte = NormalizarTexto(segmentoOriginal).Replace("4:0O", "4:00");

            List<int> dias = DetectarDiasEnTexto(parte);
            if (dias.Count == 0)
                continue;

            // "mañana" → 07:00-12:00
            if (parte.Contains("MANANA"))
            {
                foreach (int dia in dias)
                    AgregarBloque(bloques, dia, HoraInicioDia, HoraFinManana);
            }

            // Rango explícito: "Xhora a Yhora"
            Match desdeHasta = Regex.Match(
                parte,
                @"(?<inicio>\d{1,2}(:\d{2})?\s*(AM|PM)?|12M)\s*A\s*(?<fin>\d{1,2}(:\d{2})?\s*(AM|PM)?|12M)"
            );
            if (desdeHasta.Success)
            {
                string inicio = ConvertirHora(desdeHasta.Groups["inicio"].Value);
                string fin = ConvertirHora(desdeHasta.Groups["fin"].Value);
                foreach (int dia in dias)
                    AgregarBloque(bloques, dia, inicio, fin);
            }

            // "Xhora en adelante" → desde esa hora hasta fin
            Match enAdelante = Regex.Match(
                parte,
                @"(?<inicio>\d{1,2}(:\d{2})?\s*(AM|PM)?)\s*(EN ADELANTE|PM EN ADELANTE)"
            );
            if (enAdelante.Success)
            {
                string inicio = ConvertirHora(enAdelante.Groups["inicio"].Value);
                foreach (int dia in dias)
                    AgregarBloque(bloques, dia, inicio, HoraFinDia);
            }

            // "después de las Xhora" → desde esa hora hasta fin
            Match despues = Regex.Match(
                parte,
                @"DESPUES\s+DE\s+LAS?\s+(?<inicio>\d{1,2}(:\d{2})?\s*(AM|PM)?)"
            );
            if (despues.Success)
            {
                string inicio = ConvertirHora(despues.Groups["inicio"].Value);
                foreach (int dia in dias)
                    AgregarBloque(bloques, dia, inicio, HoraFinDia);
            }

            // "hasta las Xhora" → 07:00 hasta esa hora
            Match hasta = Regex.Match(
                parte,
                @"HASTA\s+LAS?\s+(?<fin>\d{1,2}(:\d{2})?\s*(AM|PM)?)"
            );
            if (hasta.Success)
            {
                string fin = ConvertirHora(hasta.Groups["fin"].Value);
                foreach (int dia in dias)
                    AgregarBloque(bloques, dia, HoraInicioDia, fin);
            }
        }
    }

    private static void AgregarRangoDias(
        List<DisponibilidadBloque> bloques,
        int diaInicio,
        int diaFin,
        string horaInicio,
        string horaFin)
    {
        for (int dia = diaInicio; dia <= diaFin; dia++)
            AgregarBloque(bloques, dia, horaInicio, horaFin);
    }

    private static void AgregarBloque(
        List<DisponibilidadBloque> bloques,
        int dia,
        string horaInicio,
        string horaFin)
    {
        if (dia < 1 || dia > 6)
            return;

        if (!EsHoraValida(horaInicio) || !EsHoraValida(horaFin))
            return;

        bloques.Add(new DisponibilidadBloque(dia, horaInicio, horaFin));
    }

    private static int ConvertirDiaTextoANumero(string dia)
    {
        string normalizado = NormalizarTexto(dia);

        return normalizado switch
        {
            "LUNES" => 1,
            "MARTES" => 2,
            "MIERCOLES" => 3,
            "JUEVES" => 4,
            "VIERNES" => 5,
            "SABADO" => 6,
            _ => 0
        };
    }

    private static string ConvertirHora(string hora)
    {
        string texto = NormalizarTexto(hora)
            .Replace(" ", "")
            .Replace(".", "")
            .Replace("12M", "12:00")
            .Replace("0O", "00");

        Match match = Regex.Match(texto, @"(?<hora>\d{1,2})(:(?<minuto>\d{2}))?(?<ampm>AM|PM)?");

        if (!match.Success)
            return hora;

        int horas = int.Parse(match.Groups["hora"].Value);
        int minutos = match.Groups["minuto"].Success
            ? int.Parse(match.Groups["minuto"].Value)
            : 0;

        string ampm = match.Groups["ampm"].Value;

        if (ampm == "PM" && horas < 12)
            horas += 12;

        if (ampm == "AM" && horas == 12)
            horas = 0;

        return $"{horas:00}:{minutos:00}";
    }

    private static bool EsHoraValida(string hora)
    {
        return Regex.IsMatch(hora, @"^\d{2}:\d{2}$");
    }

    internal static string NormalizarTexto(string texto)
    {
        string textoSinEspaciosDobles = string.Join(
            ' ',
            texto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)
        );

        string textoNormalizado = textoSinEspaciosDobles.Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();

        foreach (char caracter in textoNormalizado)
        {
            UnicodeCategory categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);

            if (categoria != UnicodeCategory.NonSpacingMark)
                builder.Append(caracter);
        }

        return builder.ToString()
            .Normalize(NormalizationForm.FormC)
            .ToUpperInvariant();
    }
}

/// <summary>
/// Elemento resultante de parsear una hoja horizontal: nombre identificado, bloques detectados y mensajes de validación.
/// </summary>
internal class DisponibilidadExcelItem
{
    public string NombreDocente { get; set; } = string.Empty;
    public string TextoOriginal { get; set; } = string.Empty;
    public List<DisponibilidadBloque> Bloques { get; set; } = new();
    public List<string> Mensajes { get; set; } = new();
}

/// <summary>
/// Resultado de leer la hoja vertical "Mi Disponibilidad" (un docente por archivo).
/// </summary>
internal class MiDisponibilidadExcelItem
{
    public string NombreDocente { get; set; } = string.Empty;
    public string TipoContrato { get; set; } = string.Empty;
    public List<string> NombresMaterias { get; set; } = new();
    public string TextoDisponibilidad { get; set; } = string.Empty;
    public List<DisponibilidadBloque> Bloques { get; set; } = new();
    public List<string> Mensajes { get; set; } = new();
}

/// <summary>
/// Representa un bloque de disponibilidad (día, hora inicio, hora fin).
/// </summary>
internal record DisponibilidadBloque(
    int DiaSemana,
    string HoraInicio,
    string HoraFin
);
