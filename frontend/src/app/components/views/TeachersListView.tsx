import { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle, CardContent } from "../Card";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "../Table";
import { Badge } from "../Badge";
import { Button } from "../Button";
import { Input } from "../Input";
import { Select } from "../Select";
import {
  Search,
  Plus,
  BookOpen,
  Edit,
  Users,
  AlertCircle,
  TrendingUp,
  Loader2,
  RefreshCw,
  X,
  Trash2,
  Upload,
} from "lucide-react";
import api from "../../../services/api";
import {
  type Docente,
  type AsignaturaHabilitada,
  obtenerDocentes,
  getAsignaturasHabilitadas,
  habilitarAsignatura,
  desvincularAsignatura,
} from "../../../services/teachersService";
import { subjectService } from "../../../services/subject.service";
import type { Subject } from "../../../services/subject.service";

// ─── Modal de materias habilitadas ───────────────────────────────────────────

interface MateriasModalProps {
  docente: Docente;
  onClose: () => void;
}

function MateriasModal({ docente, onClose }: MateriasModalProps) {
  const [habilitadas, setHabilitadas] = useState<AsignaturaHabilitada[]>([]);
  const [catalogo, setCatalogo] = useState<Subject[]>([]);
  const [loadingDatos, setLoadingDatos] = useState(true);
  const [searchCatalogo, setSearchCatalogo] = useState("");
  const [agregando, setAgregando] = useState<string | null>(null);
  const [eliminando, setEliminando] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    cargarDatos();
  }, [docente.idDocente]);

  const cargarDatos = async () => {
    setLoadingDatos(true);
    setError(null);
    try {
      const [habs, cat] = await Promise.all([
        getAsignaturasHabilitadas(docente.idDocente),
        subjectService.getAll(),
      ]);
      setHabilitadas(habs);
      setCatalogo(cat);
    } catch {
      setError("No se pudieron cargar los datos. Verifique la conexión.");
    } finally {
      setLoadingDatos(false);
    }
  };

  const handleAgregar = async (idAsignatura: string) => {
    setAgregando(idAsignatura);
    setError(null);
    try {
      const nueva = await habilitarAsignatura(docente.idDocente, idAsignatura);
      setHabilitadas((prev) => [...prev, nueva]);
      setSearchCatalogo("");
    } catch (e: any) {
      setError(e.response?.data?.mensaje ?? "Error al agregar la materia.");
    } finally {
      setAgregando(null);
    }
  };

  const handleEliminar = async (idAsignatura: string) => {
    setEliminando(idAsignatura);
    setError(null);
    try {
      await desvincularAsignatura(docente.idDocente, idAsignatura);
      setHabilitadas((prev) => prev.filter((h) => h.idAsignatura !== idAsignatura));
    } catch (e: any) {
      setError(e.response?.data?.mensaje ?? "Error al quitar la materia.");
    } finally {
      setEliminando(null);
    }
  };

  const idsHabilitados = new Set(habilitadas.map((h) => h.idAsignatura));

  const resultadosBusqueda = searchCatalogo.trim()
    ? catalogo.filter(
        (s) =>
          !idsHabilitados.has(s.idAsignatura) &&
          s.nombre.toLowerCase().includes(searchCatalogo.toLowerCase())
      )
    : [];

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-lg w-full max-w-2xl flex flex-col max-h-[85vh]">

        {/* Header del modal */}
        <div className="px-6 py-4 border-b border-[#E8E8E8] flex items-center justify-between bg-[#003087] rounded-t-lg">
          <div>
            <h2 className="text-base font-semibold text-white">Materias habilitadas</h2>
            <p className="text-xs text-white/70 mt-0.5">{docente.nombre}</p>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 hover:bg-white/20 rounded transition-colors text-white"
          >
            <X size={18} />
          </button>
        </div>

        {/* Cuerpo */}
        <div className="flex-1 overflow-y-auto p-6 space-y-5">
          {error && (
            <div className="px-3 py-2 bg-[#C0392B]/10 border border-[#C0392B]/30 rounded text-sm text-[#C0392B]">
              {error}
            </div>
          )}

          {/* Buscador del catálogo */}
          <div>
            <label className="block text-sm font-medium text-[#333333] mb-2">
              Agregar materia del catálogo
            </label>
            <div className="relative">
              <Search
                className="absolute left-3 top-1/2 -translate-y-1/2 text-[#999999]"
                size={16}
              />
              <Input
                placeholder="Buscar materia por nombre..."
                value={searchCatalogo}
                onChange={(e) => setSearchCatalogo(e.target.value)}
                className="pl-9"
              />
            </div>

            {/* Resultados del buscador */}
            {resultadosBusqueda.length > 0 && (
              <div className="mt-1 border border-[#E8E8E8] rounded overflow-hidden">
                {resultadosBusqueda.slice(0, 6).map((s) => (
                  <div
                    key={s.idAsignatura}
                    className="flex items-center justify-between px-3 py-2 border-b border-[#F0F0F0] last:border-b-0 hover:bg-[#F5F5F5] transition-colors"
                  >
                    <div>
                      <p className="text-sm text-[#333333]">{s.nombre}</p>
                      <p className="text-xs text-[#999999]">
                        {s.codigo} · Sem. {s.semestre} · {s.creditos} cr.
                      </p>
                    </div>
                    <button
                      onClick={() => handleAgregar(s.idAsignatura)}
                      disabled={agregando === s.idAsignatura}
                      className="flex items-center gap-1 px-3 py-1 text-xs font-medium text-white bg-[#1A6BBF] rounded hover:bg-[#003087] transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      {agregando === s.idAsignatura ? (
                        <Loader2 size={12} className="animate-spin" />
                      ) : (
                        <Plus size={12} />
                      )}
                      Agregar
                    </button>
                  </div>
                ))}
                {resultadosBusqueda.length > 6 && (
                  <p className="px-3 py-2 text-xs text-[#999999]">
                    {resultadosBusqueda.length - 6} resultados más. Refina la búsqueda.
                  </p>
                )}
              </div>
            )}
            {searchCatalogo.trim() && resultadosBusqueda.length === 0 && !loadingDatos && (
              <p className="mt-1 text-xs text-[#999999]">
                No se encontraron materias disponibles para agregar.
              </p>
            )}
          </div>

          {/* Lista de habilitadas */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-sm font-medium text-[#333333]">
                Habilitadas actualmente
              </label>
              {!loadingDatos && (
                <span className="text-xs text-[#999999]">
                  {habilitadas.length} materia{habilitadas.length !== 1 ? "s" : ""}
                </span>
              )}
            </div>

            {loadingDatos ? (
              <div className="flex items-center justify-center py-8">
                <Loader2 className="w-6 h-6 animate-spin text-[#1A6BBF]" />
                <span className="ml-2 text-sm text-[#666666]">Cargando...</span>
              </div>
            ) : habilitadas.length === 0 ? (
              <div className="text-center py-8 border border-dashed border-[#CCCCCC] rounded">
                <BookOpen className="mx-auto text-[#CCCCCC]" size={28} />
                <p className="mt-2 text-sm text-[#999999]">
                  Este docente no tiene materias habilitadas.
                </p>
              </div>
            ) : (
              <div className="border border-[#E8E8E8] rounded overflow-hidden">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Materia</TableHead>
                      <TableHead>Sem.</TableHead>
                      <TableHead>Cr.</TableHead>
                      <TableHead>Fuente</TableHead>
                      <TableHead></TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {habilitadas.map((h) => (
                      <TableRow key={h.idAsignatura} striped>
                        <TableCell>
                          <p className="text-sm font-medium text-[#333333]">{h.nombre}</p>
                          <p className="text-xs text-[#999999]">{h.codigo}</p>
                        </TableCell>
                        <TableCell className="text-[#666666]">{h.semestre}</TableCell>
                        <TableCell className="text-[#666666]">{h.creditos}</TableCell>
                        <TableCell>
                          <Badge variant={h.fuente === "Manual" ? "info" : "secondary"}>
                            {h.fuente}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <button
                            onClick={() => handleEliminar(h.idAsignatura)}
                            disabled={eliminando === h.idAsignatura}
                            className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#C0392B] disabled:opacity-50 disabled:cursor-not-allowed"
                            title="Quitar materia"
                          >
                            {eliminando === h.idAsignatura ? (
                              <Loader2 size={14} className="animate-spin" />
                            ) : (
                              <Trash2 size={14} />
                            )}
                          </button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="px-6 py-4 border-t border-[#E8E8E8] flex justify-end">
          <button
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-[#666666] bg-[#F5F5F5] rounded hover:bg-[#E8E8E8] transition-colors"
          >
            Cerrar
          </button>
        </div>
      </div>
    </div>
  );
}

// ─── Vista principal ──────────────────────────────────────────────────────────

export function TeachersListView() {
  const [docentes, setDocentes] = useState<Docente[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [contractFilter, setContractFilter] = useState("");
  const [docenteModal, setDocenteModal] = useState<Docente | null>(null);
  const [importando, setImportando] = useState(false);
  const [importResult, setImportResult] = useState<{ mensaje: string; tipo: "success" | "error" } | null>(null);

  useEffect(() => {
    cargarDocentes();
  }, []);

  const cargarDocentes = async () => {
    setLoading(true);
    try {
      const data = await obtenerDocentes();
      setDocentes(data);
    } catch (error) {
      console.error("Error cargando docentes:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleImportExcel = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    e.target.value = "";
    setImportando(true);
    setImportResult(null);
    try {
      const formData = new FormData();
      formData.append("archivo", file);
      const response = await api.post("/profesores/curriculos/importar-excel", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      const res = response.data;
      const msg =
        `Importación completada: ${res.docentesProcesados ?? res.registrosProcesados ?? "?"} docentes procesados` +
        (res.errores?.length ? `. ${res.errores.length} advertencias.` : ".");
      setImportResult({ mensaje: msg, tipo: "success" });
      cargarDocentes();
    } catch (err: any) {
      setImportResult({
        mensaje: err.response?.data?.mensaje || "Error al importar el archivo.",
        tipo: "error",
      });
    } finally {
      setImportando(false);
    }
  };

  const filteredDocentes = docentes.filter((d) => {
    const matchesSearch = d.nombre.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesContract = !contractFilter || d.tipoContrato === contractFilter;
    return matchesSearch && matchesContract;
  });

  const stats = {
    total: docentes.length,
    tc: docentes.filter((d) => d.tipoContrato === "TC").length,
    tp: docentes.filter((d) => d.tipoContrato === "TP").length,
  };

  if (loading) {
    return (
      <div className="flex-1 p-6 flex items-center justify-center bg-[#F5F5F5]">
        <div className="text-center">
          <Loader2 className="w-8 h-8 animate-spin text-[#1A6BBF] mx-auto" />
          <p className="mt-4 text-[#666666]">Cargando docentes...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 p-6 space-y-6 overflow-auto bg-[#F5F5F5]">

      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-medium text-[#333333]">Gestión de Docentes</h1>
          <p className="text-sm text-[#666666] mt-1">
            Administración de carga académica y disponibilidad
          </p>
        </div>
        <div className="flex gap-3">
          <Button variant="secondary" onClick={cargarDocentes} className="gap-2">
            <RefreshCw size={20} />
            Actualizar
          </Button>
          <label>
            <input
              type="file"
              accept=".xlsx,.xls"
              className="hidden"
              onChange={handleImportExcel}
              disabled={importando}
            />
            <Button
              variant="secondary"
              className="gap-2 cursor-pointer"
              disabled={importando}
              onClick={(e) => {
                e.preventDefault();
                (e.currentTarget.closest("label") as HTMLLabelElement)
                  ?.querySelector("input")
                  ?.click();
              }}
            >
              {importando ? <Loader2 className="animate-spin" size={20} /> : <Upload size={20} />}
              Importar Excel
            </Button>
          </label>
          <Button className="gap-2">
            <Plus size={20} />
            Nuevo docente
          </Button>
        </div>
      </div>

      {importResult && (
        <div
          className={`p-3 rounded border text-sm ${
            importResult.tipo === "success"
              ? "bg-[#1A7A4A]/10 border-[#1A7A4A]/30 text-[#1A7A4A]"
              : "bg-[#C0392B]/10 border-[#C0392B]/30 text-[#C0392B]"
          }`}
        >
          <div className="flex items-center justify-between">
            <span>{importResult.mensaje}</span>
            <button onClick={() => setImportResult(null)} className="ml-2 hover:opacity-70">
              <X size={16} />
            </button>
          </div>
        </div>
      )}

      {/* Stats */}
      <div className="grid grid-cols-3 gap-6">
        <Card>
          <CardContent className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-lg bg-[#1A6BBF]/10 flex items-center justify-center">
              <TrendingUp className="text-[#1A6BBF]" size={24} />
            </div>
            <div>
              <p className="text-sm text-[#666666]">Total docentes</p>
              <p className="text-2xl font-medium text-[#333333]">{stats.total}</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-lg bg-[#003087]/10 flex items-center justify-center">
              <Users className="text-[#003087]" size={24} />
            </div>
            <div>
              <p className="text-sm text-[#666666]">Tiempo completo</p>
              <p className="text-2xl font-medium text-[#333333]">{stats.tc}</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-lg bg-[#E8A020]/10 flex items-center justify-center">
              <AlertCircle className="text-[#E8A020]" size={24} />
            </div>
            <div>
              <p className="text-sm text-[#666666]">Tiempo parcial</p>
              <p className="text-2xl font-medium text-[#333333]">{stats.tp}</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Tabla */}
      <Card>
        <CardHeader>
          <CardTitle>Lista de Docentes</CardTitle>
          <div className="grid grid-cols-2 gap-4 mt-4">
            <div className="relative">
              <Search
                className="absolute left-3 top-1/2 -translate-y-1/2 text-[#999999]"
                size={20}
              />
              <Input
                placeholder="Buscar por nombre..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select
              placeholder="Filtrar por contrato"
              value={contractFilter}
              onChange={(e) => setContractFilter(e.target.value)}
              options={[
                { value: "TC", label: "Tiempo completo" },
                { value: "TP", label: "Tiempo parcial" },
              ]}
            />
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Docente</TableHead>
                <TableHead>Tipo de contrato</TableHead>
                <TableHead>Máx. asignaturas</TableHead>
                <TableHead>Acciones</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredDocentes.length === 0 ? (
                <TableRow>
                  <TableCell className="text-center text-[#999999] py-8">
                    No se encontraron docentes.
                  </TableCell>
                </TableRow>
              ) : (
                filteredDocentes.map((docente) => (
                  <TableRow key={docente.idDocente} striped>
                    <TableCell>
                      <p className="font-medium text-[#333333]">{docente.nombre}</p>
                      {docente.identificacion && (
                        <p className="text-xs text-[#999999]">{docente.identificacion}</p>
                      )}
                    </TableCell>
                    <TableCell>
                      <Badge variant={docente.tipoContrato === "TC" ? "info" : "secondary"}>
                        {docente.tipoContrato === "TC" ? "Tiempo completo" : "Tiempo parcial"}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-[#666666]">{docente.maxAsignaturas}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => setDocenteModal(docente)}
                          className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A6BBF]"
                          title="Ver materias habilitadas"
                        >
                          <BookOpen size={16} />
                        </button>
                        <button
                          className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A6BBF]"
                          title="Editar docente"
                        >
                          <Edit size={16} />
                        </button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Modal de materias */}
      {docenteModal && (
        <MateriasModal
          docente={docenteModal}
          onClose={() => setDocenteModal(null)}
        />
      )}
    </div>
  );
}
