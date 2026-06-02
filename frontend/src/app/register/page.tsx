"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";
import { Building2, Loader2, UserPlus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { confirmEmail, register } from "@/lib/api";

export default function RegisterPage() {
  const [email, setEmail] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [password, setPassword] = useState("");
  const [verificationToken, setVerificationToken] = useState("");
  const [userId, setUserId] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setMessage("");

    try {
      const result = await register({ email, password, displayName });
      setUserId(result.userId);
      setVerificationToken(result.emailVerificationToken);
      setMessage("Registration created. Verification token is ready below.");
    } catch {
      setMessage("Registration failed.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleConfirm() {
    if (!userId || !verificationToken) {
      return;
    }

    setIsSubmitting(true);
    try {
      await confirmEmail({ userId, token: verificationToken });
      setMessage("Email verified. You can log in now.");
    } catch {
      setMessage("Email verification failed.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-6 text-foreground">
      <form onSubmit={handleSubmit} className="w-full max-w-lg rounded-md border border-border bg-surface p-6">
        <div className="flex items-center gap-3 border-b border-border pb-4">
          <Building2 className="h-5 w-5 text-accent" aria-hidden="true" />
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Clinic account</p>
            <h1 className="text-xl font-semibold">Register</h1>
          </div>
        </div>
        <div className="mt-5 grid gap-4">
          <label className="grid gap-2 text-sm font-medium">
            Display name
            <input className="h-10 rounded-md border border-border bg-background px-3" value={displayName} onChange={(event) => setDisplayName(event.target.value)} required />
          </label>
          <label className="grid gap-2 text-sm font-medium">
            Email
            <input className="h-10 rounded-md border border-border bg-background px-3" value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
          </label>
          <label className="grid gap-2 text-sm font-medium">
            Password
            <input className="h-10 rounded-md border border-border bg-background px-3" value={password} onChange={(event) => setPassword(event.target.value)} type="password" required />
          </label>
          {verificationToken ? (
            <label className="grid gap-2 text-sm font-medium">
              Verification token
              <textarea className="min-h-24 rounded-md border border-border bg-background px-3 py-2 font-mono text-xs" value={verificationToken} onChange={(event) => setVerificationToken(event.target.value)} />
            </label>
          ) : null}
          {message ? <p className="rounded-md bg-muted px-3 py-2 text-sm text-muted-foreground">{message}</p> : null}
          <div className="flex flex-wrap gap-2">
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" /> : <UserPlus className="h-4 w-4" aria-hidden="true" />}
              Register
            </Button>
            <Button type="button" variant="outline" disabled={!verificationToken || isSubmitting} onClick={handleConfirm}>
              Verify
            </Button>
            <Button type="button" variant="ghost" asChild>
              <Link href="/login">Login</Link>
            </Button>
          </div>
        </div>
      </form>
    </main>
  );
}
