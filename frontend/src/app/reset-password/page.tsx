"use client";

import Link from "next/link";
import { FormEvent, Suspense, useState } from "react";
import { useSearchParams } from "next/navigation";
import { KeyRound, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { resetPassword } from "@/lib/api";

function ResetPasswordForm() {
  const searchParams = useSearchParams();
  const [email, setEmail] = useState(searchParams.get("email") ?? "");
  const [token, setToken] = useState(searchParams.get("token") ?? "");
  const [newPassword, setNewPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setMessage("");

    try {
      await resetPassword({ email, token, newPassword });
      setMessage("Password reset. You can log in now.");
    } catch {
      setMessage("Password reset failed.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-6 text-foreground">
      <form onSubmit={handleSubmit} className="w-full max-w-md rounded-md border border-border bg-surface p-6">
        <div className="flex items-center gap-3 border-b border-border pb-4">
          <KeyRound className="h-5 w-5 text-accent" aria-hidden="true" />
          <h1 className="text-xl font-semibold">Reset Password</h1>
        </div>
        <div className="mt-5 grid gap-4">
          <label className="grid gap-2 text-sm font-medium">
            Email
            <input className="h-10 rounded-md border border-border bg-background px-3" value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
          </label>
          <label className="grid gap-2 text-sm font-medium">
            Token
            <textarea className="min-h-24 rounded-md border border-border bg-background px-3 py-2 font-mono text-xs" value={token} onChange={(event) => setToken(event.target.value)} required />
          </label>
          <label className="grid gap-2 text-sm font-medium">
            New password
            <input className="h-10 rounded-md border border-border bg-background px-3" value={newPassword} onChange={(event) => setNewPassword(event.target.value)} type="password" required />
          </label>
          {message ? <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">{message}</p> : null}
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" /> : null}
            Reset
          </Button>
          <Button type="button" variant="ghost" asChild>
            <Link href="/login">Login</Link>
          </Button>
        </div>
      </form>
    </main>
  );
}

export default function ResetPasswordPage() {
  return (
    <Suspense>
      <ResetPasswordForm />
    </Suspense>
  );
}
