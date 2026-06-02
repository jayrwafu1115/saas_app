"use client";

import Link from "next/link";
import { FormEvent, Suspense, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { KeyRound, Loader2, LogIn } from "lucide-react";
import { Button } from "@/components/ui/button";
import { login } from "@/lib/api";
import { useAuthStore } from "@/store/auth-store";

function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const setAuth = useAuthStore((state) => state.setAuth);
  const [email, setEmail] = useState("superadmin@clinic.local");
  const [password, setPassword] = useState("SuperAdmin123!");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setError("");

    try {
      const auth = await login({ email, password });
      setAuth(auth);
      router.push(searchParams.get("returnTo") ?? "/tenants");
    } catch {
      setError("Login failed. Confirm the email and password are correct.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-6 text-foreground">
      <form onSubmit={handleSubmit} className="w-full max-w-md rounded-md border border-border bg-surface p-6">
        <div className="flex items-center gap-3 border-b border-border pb-4">
          <KeyRound className="h-5 w-5 text-accent" aria-hidden="true" />
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Authentication</p>
            <h1 className="text-xl font-semibold">Login</h1>
          </div>
        </div>
        <div className="mt-5 grid gap-4">
          <label className="grid gap-2 text-sm font-medium">
            Email
            <input className="h-10 rounded-md border border-border bg-background px-3" value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
          </label>
          <label className="grid gap-2 text-sm font-medium">
            Password
            <input className="h-10 rounded-md border border-border bg-background px-3" value={password} onChange={(event) => setPassword(event.target.value)} type="password" required />
          </label>
          {error ? <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">{error}</p> : null}
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" /> : <LogIn className="h-4 w-4" aria-hidden="true" />}
            Login
          </Button>
          <div className="flex items-center justify-between text-sm">
            <Link className="text-accent" href="/register">Register</Link>
            <Link className="text-accent" href="/forgot-password">Forgot password</Link>
          </div>
        </div>
      </form>
    </main>
  );
}

export default function LoginPage() {
  return (
    <Suspense>
      <LoginForm />
    </Suspense>
  );
}
