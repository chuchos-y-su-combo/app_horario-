using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace ApplicationSchedule.Infrastructure.Services;

internal static class DisponibilidadExcelParser
{
    private const string HoraInicioDia = "07:00";
    private const string HoraFinDia = "22:30";
    private const string HoraFinSinNoche = "18:00";
    private const string HoraFinManana = "12:00";
    private const string HoraInicioNoche = "18:00";

    public static List<DisponibilidadExcelItem> LeerHoja(ClosedXML.Excel.IXLWorksheet worksheet)
    {
        return TieneFormatoNormalizado(worksheet)
            ? LeerFormatoNormalizado(worksheet)
            : LeerFormatoActualCoordinacion(worksheet);
    }

    private static bool TieneFormatoNormalizado(ClosedXML.Excel.IXLWorksheet worksheet)
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
            {
                continue;
            }

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

    private static List<DisponibilidadExcelItem> LeerFormatoActualCoordinacion(ClosedXML.Excel.IXLWorksheet worksheet)
    {
        var resultado = new List<DisponibilidadExcelItem>();

        int ultimaColumna = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

        for (int columna = 1; columna <= ultimaColumna; columna++)
        {
            string nombreDocente = worksheet.Cell(1, columna).GetString().Trim();
            string textoDisponibilidad = worksheet.Cell(2, columna).GetString().Trim();

            if (string.IsNullOrWhiteSpace(nombreDocente))
            {
                continue;
            }

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

    private static List<DisponibilidadBloque> InterpretarTextoLibre(
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

        string finDia = texto.Contains("EVITAR NOCHES")
            ? HoraFinSinNoche
            : HoraFinDia;

        if (texto.Contains("TODO EL DIA L A J") ||
            texto.Contains("L A J TODO EL DIA") ||
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
        {
            AgregarBloque(bloques, 4, HoraInicioDia, finDia);
        }

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
        {
            AgregarBloque(bloques, 5, HoraInicioNoche, HoraFinDia);
        }

        if (texto.Contains("DESPUES DE LAS 6PM"))
        {
            AgregarRangoDias(bloques, 1, 4, "18:00", HoraFinDia);
        }

        if (texto.Contains("DE 7AM A 9AM"))
        {
            AgregarRangoDias(bloques, 1, 5, "07:00", "09:00");
        }

        if (texto.Contains("LUNES A VIERNES DE 7AM A 10AM"))
        {
            AgregarRangoDias(bloques, 1, 5, "07:00", "10:00");
        }

        if (texto.Contains("LUNES A JUEVES DE 6PM A 10PM"))
        {
            AgregarRangoDias(bloques, 1, 4, "18:00", "22:00");
        }

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

    private static void InterpretarLineasConDia(string textoOriginal, List<DisponibilidadBloque> bloques)
    {
        string[] partes = textoOriginal
            .Replace(".", "\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (string parteOriginal in partes)
        {
            string parte = NormalizarTexto(parteOriginal)
                .Replace("4:0O", "4:00");

            int dia = DetectarDiaEnTexto(parte);

            if (dia == 0)
            {
                continue;
            }

            if (parte.Contains("MANANA"))
            {
                AgregarBloque(bloques, dia, HoraInicioDia, HoraFinManana);
            }

            Match desdeHasta = Regex.Match(
                parte,
                @"(?<inicio>\d{1,2}(:\d{2})?\s*(AM|PM)?|12M)\s*A\s*(?<fin>\d{1,2}(:\d{2})?\s*(AM|PM)?|12M)"
            );

            if (desdeHasta.Success)
            {
                string inicio = ConvertirHora(desdeHasta.Groups["inicio"].Value);
                string fin = ConvertirHora(desdeHasta.Groups["fin"].Value);

                AgregarBloque(bloques, dia, inicio, fin);
            }

            Match enAdelante = Regex.Match(
                parte,
                @"(?<inicio>\d{1,2}(:\d{2})?\s*(AM|PM)?)\s*(EN ADELANTE|PM EN ADELANTE)"
            );

            if (enAdelante.Success)
            {
                string inicio = ConvertirHora(enAdelante.Groups["inicio"].Value);

                AgregarBloque(bloques, dia, inicio, HoraFinDia);
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
        {
            AgregarBloque(bloques, dia, horaInicio, horaFin);
        }
    }

    private static void AgregarBloque(
        List<DisponibilidadBloque> bloques,
        int dia,
        string horaInicio,
        string horaFin)
    {
        if (dia < 1 || dia > 6)
        {
            return;
        }

        if (!EsHoraValida(horaInicio) || !EsHoraValida(horaFin))
        {
            return;
        }

        bloques.Add(new DisponibilidadBloque(dia, horaInicio, horaFin));
    }

    private static int DetectarDiaEnTexto(string texto)
    {
        if (texto.Contains("LUNES")) return 1;
        if (texto.Contains("MARTES")) return 2;
        if (texto.Contains("MIERCOLES")) return 3;
        if (texto.Contains("JUEVES")) return 4;
        if (texto.Contains("VIERNES")) return 5;
        if (texto.Contains("SABADO")) return 6;

        return 0;
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
        {
            return hora;
        }

        int horas = int.Parse(match.Groups["hora"].Value);
        int minutos = match.Groups["minuto"].Success
            ? int.Parse(match.Groups["minuto"].Value)
            : 0;

        string ampm = match.Groups["ampm"].Value;

        if (ampm == "PM" && horas < 12)
        {
            horas += 12;
        }

        if (ampm == "AM" && horas == 12)
        {
            horas = 0;
        }

        return $"{horas:00}:{minutos:00}";
    }

    private static bool EsHoraValida(string hora)
    {
        return Regex.IsMatch(hora, @"^\d{2}:\d{2}$");
    }

    private static string NormalizarTexto(string texto)
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
            {
                builder.Append(caracter);
            }
        }

        return builder.ToString()
            .Normalize(NormalizationForm.FormC)
            .ToUpperInvariant();
    }
}

internal class DisponibilidadExcelItem
{
    public string NombreDocente { get; set; } = string.Empty;

    public string TextoOriginal { get; set; } = string.Empty;

    public List<DisponibilidadBloque> Bloques { get; set; } = new();

    public List<string> Mensajes { get; set; } = new();
}

internal record DisponibilidadBloque(
    int DiaSemana,
    string HoraInicio,
    string HoraFin
);