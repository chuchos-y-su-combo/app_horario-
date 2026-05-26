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
import { ConfirmModal } from "../Modal";
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
  const [docenteAEliminar, setDocenteAEliminar] = useState<Docente | null>(null);
  const [eliminandoDocente, setEliminandoDocente] = useState(false);
  const [errorEliminar, setErrorEliminar] = useState<string | null>(null);
  const [docenteEditar, setDocenteEditar] = useState<Docente | null>(null);
  const [editNombre, setEditNombre] = useState("");
  const [editIdentificacion, setEditIdentificacion] = useState("");
  const [editTipoContrato, setEditTipoContrato] = useState("TC");
  const [editando, setEditando] = useState(false);
  const [errorEditar, setErrorEditar] = useState<string | null>(null);
  const [importando, setImportando] = useState(false);
  const [importProgress, setImportProgress] = useState<{ actual: number; total: number; nombre: string } | null>(null);
  const [importSummary, setImportSummary] = useState<{
    procesados: number;
    exitosos: number;
    errores: { archivo: string; mensaje: string }[];
  } | null>(null);
  const [showEliminarTodosModal, setShowEliminarTodosModal] = useState(false);
  const [eliminandoTodos, setEliminandoTodos] = useState(false);

  useEffect(() => {
    cargarDocentes();
  }, []);

  const cargarDocentes = async () => {
    setLoading(true);
    try {
      const data = await obtenerDocentes();
      setDocentes(data);
    } catch {
      // La UI muestra estado vacío si falla la carga
    } finally {
      setLoading(false);
    }
  };

  const abrirEditar = (docente: Docente) => {
    setDocenteEditar(docente);
    setEditNombre(docente.nombre);
    setEditIdentificacion(docente.identificacion);
    setEditTipoContrato(docente.tipoContrato);
    setErrorEditar(null);
  };

  const handleEditSubmit = async () => {
    if (!docenteEditar) return;
    setEditando(true);
    setErrorEditar(null);
    try {
      await api.put(`/profesores/${docenteEditar.idDocente}`, {
        nombre: editNombre.trim(),
        identificacion: editIdentificacion.trim(),
        tipoContrato: editTipoContrato,
      });
      setDocenteEditar(null);
      cargarDocentes();
    } catch (err: any) {
      setErrorEditar(
        err?.response?.data?.mensaje ??
        err?.response?.data?.message ??
        err?.message ??
        "Error al actualizar el docente."
      );
    } finally {
      setEditando(false);
    }
  };

  const handleEliminarDocente = async () => {
    if (!docenteAEliminar) return;
    setEliminandoDocente(true);
    setErrorEliminar(null);
    try {
      await api.delete(`/profesores/${docenteAEliminar.idDocente}`);
      setDocenteAEliminar(null);
      cargarDocentes();
    } catch (err: any) {
      setErrorEliminar(
        err?.response?.data?.mensaje ??
        err?.response?.data?.message ??
        err?.message ??
        "Error al eliminar el docente."
      );
    } finally {
      setEliminandoDocente(false);
    }
  };

  const handleImportExcel = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files ?? []);
    if (files.length === 0) return;
    e.target.value = "";

    setImportando(true);
    setImportSummary(null);

    let exitosos = 0;
    const errores: { archivo: string; mensaje: string }[] = [];

    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      setImportProgress({ actual: i + 1, total: files.length, nombre: file.name });
      try {
        const formData = new FormData();
        formData.append("archivo", file);
        await api.post("/profesores/curriculos/importar-excel", formData, {
          headers: { "Content-Type": "multipart/form-data" },
        });
        exitosos++;
      } catch (err: any) {
        errores.push({
          archivo: file.name,
          mensaje: err.response?.data?.mensaje ?? err.response?.data?.message ?? err.message ?? "Error desconocido",
        });
      }
    }

    setImportProgress(null);
    setImportando(false);
    setImportSummary({ procesados: files.length, exitosos, errores });
    cargarDocentes();
  };

  const handleEliminarTodos = async () => {
    setEliminandoTodos(true);
    try {
      await api.delete("/profesores/todos");
      setShowEliminarTodosModal(false);
      cargarDocentes();
    } catch (err: any) {
      alert(
        err?.response?.data?.mensaje ??
        err?.response?.data?.message ??
        err?.message ??
        "Error al eliminar todos los profesores."
      );
    } finally {
      setEliminandoTodos(false);
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
          <Button
            variant="destructive"
            className="gap-2"
            onClick={() => setShowEliminarTodosModal(true)}
            disabled={docentes.length === 0}
          >
            <Trash2 size={20} />
            Eliminar todos
          </Button>
          <label>
            <input
              type="file"
              accept=".xlsx,.xls"
              multiple
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

      {errorEliminar && (
        <div className="p-3 rounded border text-sm bg-[#C0392B]/10 border-[#C0392B]/30 text-[#C0392B]">
          <div className="flex items-center justify-between gap-2">
            <span>{errorEliminar}</span>
            <button onClick={() => setErrorEliminar(null)} className="hover:opacity-70 shrink-0">
              <X size={16} />
            </button>
          </div>
        </div>
      )}

      {importProgress && (
        <div className="p-3 rounded border text-sm bg-[#1A6BBF]/10 border-[#1A6BBF]/30 text-[#1A6BBF]">
          <div className="flex items-center gap-2">
            <Loader2 size={16} className="animate-spin shrink-0" />
            <span>Procesando archivo {importProgress.actual} de {importProgress.total}: {importProgress.nombre}</span>
          </div>
        </div>
      )}

      {importSummary && (
        <div
          className={`p-3 rounded border text-sm ${
            importSummary.errores.length === 0
              ? "bg-[#1A7A4A]/10 border-[#1A7A4A]/30 text-[#1A7A4A]"
              : importSummary.exitosos === 0
              ? "bg-[#C0392B]/10 border-[#C0392B]/30 text-[#C0392B]"
              : "bg-[#E8A020]/10 border-[#E8A020]/30 text-[#E8A020]"
          }`}
        >
          <div className="flex items-start justify-between gap-2">
            <div>
              <p className="font-medium">
                {importSummary.procesados} archivo{importSummary.procesados !== 1 ? "s" : ""} procesado{importSummary.procesados !== 1 ? "s" : ""}:&nbsp;
                {importSummary.exitosos} registrado{importSummary.exitosos !== 1 ? "s" : ""},&nbsp;
                {importSummary.errores.length} error{importSummary.errores.length !== 1 ? "es" : ""}
              </p>
              {importSummary.errores.length > 0 && (
                <ul className="mt-1 space-y-0.5 text-xs">
                  {importSummary.errores.map((e, i) => (
                    <li key={i}><span className="font-medium">{e.archivo}:</span> {e.mensaje}</li>
                  ))}
                </ul>
              )}
            </div>
            <button onClick={() => setImportSummary(null)} className="hover:opacity-70 shrink-0">
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
          <div className="flex items-center justify-between">
            <CardTitle>Lista de Docentes</CardTitle>
            <span className="text-sm text-[#666666]">
              {filteredDocentes.length === docentes.length
                ? `${docentes.length} docente${docentes.length !== 1 ? "s" : ""}`
                : `${filteredDocentes.length} de ${docentes.length} docentes`}
            </span>
          </div>
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
                      {docente.identificacion && !docente.identificacion.startsWith("IMP-") && (
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
                          onClick={() => abrirEditar(docente)}
                          className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#1A6BBF]"
                          title="Editar docente"
                        >
                          <Edit size={16} />
                        </button>
                        <button
                          onClick={() => setDocenteAEliminar(docente)}
                          className="p-1.5 hover:bg-[#F5F5F5] rounded transition-colors text-[#C0392B]"
                          title="Eliminar docente"
                        >
                          <Trash2 size={16} />
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

      {docenteEditar && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-lg w-full max-w-md">
            <div className="px-6 py-4 border-b border-[#E8E8E8] flex items-center justify-between bg-[#003087] rounded-t-lg">
              <h2 className="text-base font-semibold text-white">Editar docente</h2>
              <button
                onClick={() => setDocenteEditar(null)}
                className="p-1.5 hover:bg-white/20 rounded transition-colors text-white"
              >
                <X size={18} />
              </button>
            </div>

            <div className="p-6 space-y-4">
              {errorEditar && (
                <div className="px-3 py-2 bg-[#C0392B]/10 border border-[#C0392B]/30 rounded text-sm text-[#C0392B]">
                  {errorEditar}
                </div>
              )}

              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Nombre completo</label>
                <Input
                  value={editNombre}
                  onChange={(e) => setEditNombre(e.target.value)}
                  placeholder="Nombre del docente"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Identificación</label>
                <Input
                  value={editIdentificacion}
                  onChange={(e) => setEditIdentificacion(e.target.value)}
                  placeholder="Número de identificación"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-[#333333] mb-1">Tipo de contrato</label>
                <Select
                  value={editTipoContrato}
                  onChange={(e) => setEditTipoContrato(e.target.value)}
                  options={[
                    { value: "TC", label: "Tiempo completo (TC)" },
                    { value: "TP", label: "Tiempo parcial (TP)" },
                  ]}
                />
              </div>
            </div>

            <div className="px-6 py-4 border-t border-[#E8E8E8] flex justify-end gap-3">
              <button
                onClick={() => setDocenteEditar(null)}
                className="px-4 py-2 text-sm font-medium text-[#666666] bg-[#F5F5F5] rounded hover:bg-[#E8E8E8] transition-colors"
              >
                Cancelar
              </button>
              <button
                onClick={handleEditSubmit}
                disabled={editando || !editNombre.trim() || !editIdentificacion.trim()}
                className="px-4 py-2 text-sm font-medium text-white bg-[#1A6BBF] rounded hover:bg-[#003087] transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
              >
                {editando && <Loader2 size={14} className="animate-spin" />}
                {editando ? "Guardando..." : "Guardar cambios"}
              </button>
            </div>
          </div>
        </div>
      )}

      <ConfirmModal
        isOpen={!!docenteAEliminar}
        onClose={() => setDocenteAEliminar(null)}
        onConfirm={handleEliminarDocente}
        title="Eliminar docente"
        message={`¿Está seguro que desea eliminar a "${docenteAEliminar?.nombre}"? Esta acción no se puede deshacer.`}
        confirmText={eliminandoDocente ? "Eliminando..." : "Eliminar"}
        cancelText="Cancelar"
        variant="danger"
      />

      <ConfirmModal
        isOpen={showEliminarTodosModal}
        onClose={() => setShowEliminarTodosModal(false)}
        onConfirm={handleEliminarTodos}
        title="Eliminar todos los profesores"
        message="¿Estás seguro? Esta acción eliminará TODOS los docentes y sus asignaciones. Esta operación no se puede deshacer."
        confirmText={eliminandoTodos ? "Eliminando..." : "Sí, eliminar todo"}
        cancelText="Cancelar"
        variant="danger"
      />
    </div>
  );
}
