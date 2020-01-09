package com.example.alldone.model;


public class Task {
    int id;
    String title, description, priority;

    public int getId() {
        return id;
    }

    public String getId_string() {
        return String.valueOf(getId());
    }

    public void setId(int id) {
        this.id = id;
    }

    public String getPriority() {
        return priority;
    }

    public void setPriority(String priority) {
        this.priority = priority;
    }

    public String getTitle() {
        return title;
    }

    public void setTitle(String title) {
        this.title = title;
    }

    public String getDescription() {
        return description;
    }

    public void setDescription(String description) {
        this.description = description;
    }
}
