"use client";

import { useState } from "react";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { Button, Card, Input, Select, Toast } from "@/components/ui";

import { AdminError, AdminPageHeading } from "@/features/admin/admin-shared";

import { useAuth } from "@/features/auth/auth-provider";

import type { AppSetting } from "@/types/admin";

function SettingEditor({
  setting,
  onSave,
  saving,
}: {
  setting: AppSetting;
  onSave: (key: string, value: string) => void;
  saving: boolean;
}) {
  const [value, setValue] = useState(setting.value);

  const isBoolean = setting.value === "true" || setting.value === "false";

  return (
    <Card className="setting-card">
      <div>
        <h2>{setting.key}</h2>

        <p>{setting.description ?? "Application setting"}</p>
      </div>

      <div className="setting-control">
        {isBoolean ? (
          <Select
            value={value}
            onChange={(event) => setValue(event.target.value)}
          >
            <option value="true">Enabled</option>

            <option value="false">Disabled</option>
          </Select>
        ) : (
          <Input
            value={value}
            onChange={(event) => setValue(event.target.value)}
          />
        )}

        <Button disabled={saving} onClick={() => onSave(setting.key, value)}>
          Save
        </Button>
      </div>
    </Card>
  );
}

export function AdminSettingsPage() {
  const { request } = useAuth();

  const queryClient = useQueryClient();

  const [notice, setNotice] = useState<string | null>(null);

  const query = useQuery({
    queryKey: ["admin-settings"],

    queryFn: () => request<AppSetting[]>("/api/v1/admin/settings"),
  });

  const mutation = useMutation({
    mutationFn: ({ key, value }: { key: string; value: string }) =>
      request<AppSetting>(`/api/v1/admin/settings/${encodeURIComponent(key)}`, {
        method: "PUT",
        body: JSON.stringify({
          value,
        }),
      }),

    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["admin-settings"],
      });

      setNotice("Setting updated successfully.");
    },
  });

  return (
    <>
      <AdminPageHeading
        eyebrow="Institution preferences"
        title="Settings"
        description="Control the small set of institution-wide defaults used by SlateDesk."
      />

      {query.error && (
        <AdminError
          message={
            query.error instanceof Error
              ? query.error.message
              : "Unable to load settings."
          }
        />
      )}

      <div className="setting-list">
        {query.data?.map((setting) => (
          <SettingEditor
            key={`${setting.id}:${setting.value}`}
            setting={setting}
            saving={mutation.isPending}
            onSave={(key, value) =>
              mutation.mutate({
                key,
                value,
              })
            }
          />
        ))}
      </div>

      {mutation.error && (
        <AdminError
          message={
            mutation.error instanceof Error
              ? mutation.error.message
              : "Unable to update setting."
          }
        />
      )}

      {notice && <Toast message={notice} />}
    </>
  );
}
