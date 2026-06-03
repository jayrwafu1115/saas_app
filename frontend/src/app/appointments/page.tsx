"use client";

import Link from "next/link";
import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, CalendarDays, Check, LogOut, Plus, X } from "lucide-react";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { Button } from "@/components/ui/button";
import { cancelAppointment, checkInAppointment, checkOutAppointment, getAppointmentCalendar } from "@/lib/api";

type CalendarView = "daily" | "weekly" | "monthly";

export default function AppointmentCalendarPage() {
  const queryClient = useQueryClient();
  const [view, setView] = useState<CalendarView>("daily");
  const [date, setDate] = useState(new Date().toISOString().slice(0, 10));
  const appointmentsQuery = useQuery({
    queryKey: ["appointments", view, date],
    queryFn: () => getAppointmentCalendar({ view, date }),
  });
  const refresh = () => queryClient.invalidateQueries({ queryKey: ["appointments"] });
  const cancelMutation = useMutation({ mutationFn: cancelAppointment, onSuccess: refresh });
  const checkInMutation = useMutation({ mutationFn: checkInAppointment, onSuccess: refresh });
  const checkOutMutation = useMutation({ mutationFn: checkOutAppointment, onSuccess: refresh });

  return (
    <ProtectedRoute>
      <main className="min-h-screen bg-background text-foreground">
        <header className="border-b border-border bg-surface">
          <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-6 py-4">
            <div className="flex items-center gap-3">
              <Button variant="ghost" size="icon" asChild aria-label="Back">
                <Link href="/"><ArrowLeft className="h-4 w-4" aria-hidden="true" /></Link>
              </Button>
              <div>
                <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Schedule</p>
                <h1 className="text-xl font-semibold">Appointments</h1>
              </div>
            </div>
            <Button asChild>
              <Link href="/appointments/new">
                <Plus className="h-4 w-4" aria-hidden="true" />
                New
              </Link>
            </Button>
          </div>
        </header>

        <section className="mx-auto max-w-6xl px-6 py-8">
          <div className="mb-4 flex flex-wrap items-center gap-3">
            <div className="flex rounded-md border border-border bg-surface p-1">
              {(["daily", "weekly", "monthly"] as CalendarView[]).map((item) => (
                <button
                  key={item}
                  className={`h-9 rounded px-3 text-sm capitalize ${view === item ? "bg-foreground text-background" : "text-muted-foreground"}`}
                  onClick={() => setView(item)}
                  type="button"
                >
                  {item}
                </button>
              ))}
            </div>
            <input className="h-10 rounded-md border border-border bg-surface px-3 text-sm" type="date" value={date} onChange={(event) => setDate(event.target.value)} />
          </div>

          <div className="rounded-md border border-border bg-surface">
            <div className="grid grid-cols-[0.5fr_0.8fr_1fr_0.6fr_0.8fr] border-b border-border px-4 py-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
              <span>Time</span>
              <span>Status</span>
              <span>Reason</span>
              <span>Doctor</span>
              <span />
            </div>
            <div className="divide-y divide-border">
              {appointmentsQuery.data?.length ? appointmentsQuery.data.map((appointment) => (
                <div key={appointment.id} className="grid grid-cols-[0.5fr_0.8fr_1fr_0.6fr_0.8fr] items-center px-4 py-3 text-sm">
                  <span>{formatTime(appointment.startsAtUtc)}</span>
                  <span>{appointment.status}</span>
                  <span className="font-medium">{appointment.reason}</span>
                  <span className="text-muted-foreground">{appointment.doctorUserId.slice(0, 8)}</span>
                  <div className="flex justify-end gap-1">
                    <Button variant="ghost" size="icon" onClick={() => checkInMutation.mutate(appointment.id)} aria-label="Check in">
                      <Check className="h-4 w-4" aria-hidden="true" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => checkOutMutation.mutate(appointment.id)} aria-label="Check out">
                      <LogOut className="h-4 w-4" aria-hidden="true" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => cancelMutation.mutate(appointment.id)} aria-label="Cancel">
                      <X className="h-4 w-4" aria-hidden="true" />
                    </Button>
                  </div>
                </div>
              )) : (
                <div className="flex items-center gap-2 px-4 py-6 text-sm text-muted-foreground">
                  <CalendarDays className="h-4 w-4" aria-hidden="true" />
                  No appointments
                </div>
              )}
            </div>
          </div>
        </section>
      </main>
    </ProtectedRoute>
  );
}

function formatTime(value: string) {
  return new Intl.DateTimeFormat("en", { hour: "numeric", minute: "2-digit" }).format(new Date(value));
}
